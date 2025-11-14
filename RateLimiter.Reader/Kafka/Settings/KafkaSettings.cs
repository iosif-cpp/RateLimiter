using Confluent.Kafka;

namespace RateLimiter.Reader.Kafka;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string TopicName { get; set; } = "user-events";
    public string GroupId { get; set; } = "RateLimiterReader";
    public bool EnableAutoCommit { get; set; }
    public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Earliest;
}

