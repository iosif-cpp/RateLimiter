using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserRequestsKafkaGenerator.Common;
using UserRequestsKafkaGenerator.Services;

namespace UserRequestsKafkaGenerator;

public static class Program
{
    public static void Main()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                var kafkaSection = context.Configuration.GetSection("KafkaSettings");
                if (!kafkaSection.Exists())
                {
                    throw new InvalidOperationException("KafkaSettings section not found in configuration");
                }

                services.Configure<KafkaSettings>(kafkaSection);
                services.AddSingleton<KafkaProducerService>();
            })
            .Build();

        using var scope = host.Services.CreateScope();
        var kafkaService = scope.ServiceProvider.GetRequiredService<KafkaProducerService>();

        Console.WriteLine("Kafka Event Producer Started");
        Console.WriteLine("Available commands:");
        Console.WriteLine("  add <userId> <endpoint> <rpm> - Add new user task");
        Console.WriteLine("  update <userId> <currentEndpoint> <newEndpoint> <newRpm> - Update user task");
        Console.WriteLine("  remove <userId> <endpoint> - Remove specific user task");
        Console.WriteLine("  remove-all <userId> - Remove all tasks for user");
        Console.WriteLine("  list - Show current tasks");
        Console.WriteLine("  exit - Exit program");
        Console.WriteLine();

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input)) continue;

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                break;

            if (input.Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                kafkaService.DisplayCurrentTasks();
                continue;
            }

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts[0].Equals("add", StringComparison.OrdinalIgnoreCase) && parts.Length == 4)
            {
                if (int.TryParse(parts[1], out int userId) && int.TryParse(parts[3], out int rpm))
                {
                    kafkaService.AddUserTask(userId, parts[2], rpm);
                }
                else
                {
                    Console.WriteLine("Invalid parameters. Usage: add <userId> <endpoint> <rpm>");
                }
            }
            else if (parts[0].Equals("update", StringComparison.OrdinalIgnoreCase) && parts.Length == 5)
            {
                if (int.TryParse(parts[1], out int userId) && int.TryParse(parts[4], out int newRpm))
                {
                    kafkaService.UpdateUserTask(userId, parts[2], parts[3], newRpm);
                }
                else
                {
                    Console.WriteLine("Invalid parameters. Usage: update <userId> <currentEndpoint> <newEndpoint> <newRpm>");
                }
            }
            else if (parts[0].Equals("remove", StringComparison.OrdinalIgnoreCase) && parts.Length == 3)
            {
                if (int.TryParse(parts[1], out int userId))
                {
                    kafkaService.RemoveUserTask(userId, parts[2]);
                }
                else
                {
                    Console.WriteLine("Invalid user ID");
                }
            }
            else if (parts[0].Equals("remove-all", StringComparison.OrdinalIgnoreCase) && parts.Length == 2)
            {
                if (int.TryParse(parts[1], out int userId))
                {
                    kafkaService.RemoveAllUserTasks(userId);
                }
                else
                {
                    Console.WriteLine("Invalid user ID");
                }
            }
            else
            {
                Console.WriteLine("Unknown command");
            }
        }
    }
}