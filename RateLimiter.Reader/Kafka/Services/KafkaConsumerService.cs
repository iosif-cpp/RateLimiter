using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using RateLimiter.Reader.Kafka;
using RateLimiter.Reader.Kafka.Models;
using RateLimiter.Reader.Redis.Interfaces;
using RateLimiter.Writer.AppLayer.Interfaces;

namespace RateLimiter.Reader.Kafka.Services;

public class KafkaConsumerService : IDisposable, IHostedService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IRedisReaderService _redisReaderService;
    private readonly IWriterService _writerService;
    private readonly KafkaSettings _settings;
    private readonly ConsumerConfig  _config;
    private IConsumer<Ignore, string>? _consumer;
    private Thread? _consumerThread;
    
    public KafkaConsumerService(
        ILogger<KafkaConsumerService> logger,
        IRedisReaderService redisReaderService,
        IWriterService writerService,
        IOptions<KafkaSettings> kafkaOptions)
    {
        _logger = logger;
        _redisReaderService = redisReaderService;
        _writerService = writerService;
        _settings = kafkaOptions.Value ?? throw new ArgumentNullException(nameof(kafkaOptions));
        _config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = _settings.AutoOffsetReset,
            EnableAutoCommit = _settings.EnableAutoCommit
        };
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
        _consumer.Subscribe(_settings.TopicName);
        
        _consumerThread = new Thread(() => ConsumeLoop(cancellationToken))
        {
            IsBackground = true,
            Name = "ConsumerThread"
        };
        _consumerThread.Start();
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer?.Close();
        return Task.CompletedTask;
    }

    public async Task ConsumeLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer?.Consume(cancellationToken);
                    if (consumeResult == null) continue;
                    
                    if (consumeResult is { Message.Value: { } json })
                    {
                        try
                        {
                            var userEvent = JsonSerializer.Deserialize<UserEvent>(json);

                            if (userEvent is null) continue;
                            {
                                if (await _redisReaderService.IsBannedAsync(userEvent.UserId))
                                {
                                    _logger.LogInformation($"User {userEvent.UserId} has been banned.");
                                    _consumer!.Commit(consumeResult);
                                    continue;
                                }
                                

                                var currentCount = await _redisReaderService.IncrementCounterAsync(userEvent.UserId, userEvent.Endpoint);
                                var rpmLimit =
                                    await _writerService.GetRateLimitByRouteAsync(userEvent.Endpoint,
                                        cancellationToken);

                                if (currentCount > rpmLimit?.RequestsPerMinute)
                                {
                                   await _redisReaderService.BanAsync(userEvent.UserId);
                                   await _redisReaderService.DeleteCounterAsync(userEvent.UserId, userEvent.Endpoint);
                                }
                                
                                _consumer!.Commit(consumeResult);
                            }
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError(ex, "Deserialize exception: {Json}", json);
                        }
                    }
                }
                catch (ConsumeException exception)
                {
                    _logger.LogError(exception, "Consume error: {Reason}", exception.Error.Reason);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Consumer thread was cancelled");
        }
    }
    
    public void Dispose()
    {
        _consumer?.Dispose();
    }
}