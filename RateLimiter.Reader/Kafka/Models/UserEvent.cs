using System.Text.Json.Serialization;

namespace RateLimiter.Reader.Kafka.Models;

public class UserEvent
{
    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; } = string.Empty;
}