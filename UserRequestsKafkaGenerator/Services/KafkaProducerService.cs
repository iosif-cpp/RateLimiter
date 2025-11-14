using System.Collections.Concurrent;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using UserRequestsKafkaGenerator.Common;
using UserRequestsKafkaGenerator.Models;

namespace UserRequestsKafkaGenerator.Services;

public class KafkaProducerService : IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topicName;
    private readonly ConcurrentDictionary<string, UserTaskConfig> _userTasks = new();

    public KafkaProducerService(IOptions<KafkaSettings> kafkaSettings)
    {
        var settings = kafkaSettings.Value;

        if (settings == null)
        {
            throw new ArgumentException("KafkaSettings is null. Check appsettings.json configuration.", nameof(kafkaSettings));
        }

        if (string.IsNullOrWhiteSpace(settings.BootstrapServers))
        {
            throw new ArgumentException(
                $"BootstrapServers cannot be null or empty. Current value: '{settings.BootstrapServers}'. Check appsettings.json KafkaSettings section.", 
                nameof(kafkaSettings));
        }

        if (string.IsNullOrWhiteSpace(settings.TopicName))
        {
            throw new ArgumentException(
                $"TopicName cannot be null or empty. Current value: '{settings.TopicName}'. Check appsettings.json KafkaSettings section.", 
                nameof(kafkaSettings));
        }

        var config = new ProducerConfig
        {
            BootstrapServers = settings.BootstrapServers
        };

        _topicName = settings.TopicName;
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public void AddUserTask(int userId, string endpoint, int rpm)
    {
        var taskId = TaskIdCreator.GetTaskId(userId, endpoint);

        if (_userTasks.ContainsKey(taskId))
        {
            Console.WriteLine($"Task already exists: User {userId}, {endpoint}");
            return;
        }

        var taskConfig = StartUserTask(userId, endpoint, rpm);
        _userTasks[taskId] = taskConfig;
        Console.WriteLine($"Added: User {userId}, {endpoint} at {rpm} RPM");
    }

    public void UpdateUserTask(int userId, string currentEndpoint, string newEndpoint, int newRpm)
    {
        var currentTaskId = TaskIdCreator.GetTaskId(userId, currentEndpoint);
        var newTaskId = TaskIdCreator.GetTaskId(userId, newEndpoint);

        if (!_userTasks.TryGetValue(currentTaskId, out var existingTask))
        {
            Console.WriteLine($"Task not found: User {userId}, {currentEndpoint}");
            return;
        }

        if (currentEndpoint == newEndpoint)
        {
            if (existingTask.Rpm != newRpm)
            {
                existingTask.CancellationTokenSource?.Cancel();
                var newTaskConfig = StartUserTask(userId, newEndpoint, newRpm);
                _userTasks[currentTaskId] = newTaskConfig;
                Console.WriteLine($"Updated: User {userId}, {newEndpoint} at {newRpm} RPM");
            }
            else
            {
                Console.WriteLine($"No changes: User {userId}, {newEndpoint} already at {newRpm} RPM");
            }
        }
        else
        {
            if (_userTasks.ContainsKey(newTaskId))
            {
                Console.WriteLine($"Task already exists: User {userId}, {newEndpoint}");
                return;
            }

            existingTask.CancellationTokenSource?.Cancel();
            _userTasks.TryRemove(currentTaskId, out _);

            var newTaskConfig = StartUserTask(userId, newEndpoint, newRpm);
            _userTasks[newTaskId] = newTaskConfig;

            Console.WriteLine($"Updated: User {userId}, {currentEndpoint} → {newEndpoint} at {newRpm} RPM");
        }
    }

    private UserTaskConfig StartUserTask(int userId, string endpoint, int rpm)
    {
        var cts = new CancellationTokenSource();
        var taskConfig = new UserTaskConfig
        {
            UserId = userId,
            Endpoint = endpoint,
            Rpm = rpm,
            IsActive = true,
            CancellationTokenSource = cts
        };

        _ = Task.Run(async () => await SendMessagesForUser(taskConfig, cts.Token), cts.Token);
        return taskConfig;
    }

    private async Task SendMessagesForUser(UserTaskConfig config, CancellationToken cancellationToken)
    {
        if (config.Rpm <= 0)
        {
            Console.WriteLine($"RPM <= 0, skipping TaskId {config.TaskId}");
            return;
        }

        var delayBetweenMessages = TimeSpan.FromMinutes(1.0 / config.Rpm);

        while (!cancellationToken.IsCancellationRequested && config.IsActive)
        {
            try
            {
                var eventData = new Event
                {
                    UserId = config.UserId,
                    Endpoint = config.Endpoint
                };

                var message = JsonSerializer.Serialize(eventData);
                await _producer.ProduceAsync(_topicName, new Message<Null, string> { Value = message }, cancellationToken);

                await Task.Delay(delayBetweenMessages, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ProduceException<Null, string> ex)
            {
                Console.WriteLine(
                    $"Error sending message for user {config.UserId}, endpoint {config.Endpoint}: {ex.Error.Reason}");
                if (ex.Error.Code == ErrorCode.Local_InvalidArg || 
                    (ex.Error.IsError && ex.Error.Reason?.Contains("Invalid", StringComparison.OrdinalIgnoreCase) == true))
                {
                    Console.WriteLine($"Configuration error. Check BootstrapServers and TopicName settings.");
                    break;
                }
                await Task.Delay(1000, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error sending message for user {config.UserId}, endpoint {config.Endpoint}: {ex.Message}");
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    public void RemoveUserTask(int userId, string endpoint)
    {
        var taskId = TaskIdCreator.GetTaskId(userId, endpoint);

        if (_userTasks.TryRemove(taskId, out var task))
        {
            task.CancellationTokenSource?.Cancel();
            Console.WriteLine($"Removed: User {userId}, {endpoint}");
        }
        else
        {
            Console.WriteLine($"Task not found: User {userId}, {endpoint}");
        }
    }

    public void RemoveAllUserTasks(int userId)
    {
        var tasksToRemove = _userTasks.Where(t => t.Value.UserId == userId).ToList();

        if (tasksToRemove.Count == 0)
        {
            Console.WriteLine($"No tasks found for user {userId}");
            return;
        }

        foreach (var task in tasksToRemove)
        {
            if (_userTasks.TryRemove(task.Key, out var taskConfig))
            {
                taskConfig.CancellationTokenSource?.Cancel();
                Console.WriteLine($"Removed: User {userId}, {taskConfig.Endpoint}");
            }
        }
    }

    public void DisplayCurrentTasks()
    {
        Console.WriteLine("\nCurrent active tasks:");
        var groupedByUser = _userTasks.Values
            .GroupBy(t => t.UserId)
            .OrderBy(g => g.Key);

        foreach (var userGroup in groupedByUser)
        {
            Console.WriteLine($"  User {userGroup.Key}:");
            foreach (var task in userGroup.OrderBy(t => t.Endpoint))
            {
                Console.WriteLine($"    - {task.Endpoint} at {task.Rpm} RPM");
            }
        }

        Console.WriteLine();
    }

    public void Dispose()
    {
        foreach (var task in _userTasks.Values)
        {
            task.CancellationTokenSource?.Cancel();
        }

        _userTasks.Clear();
        _producer.Dispose();
    }
}