using System.Text.Json.Serialization;

namespace UserRequestsKafkaGenerator.Models;

public class Event
{
    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; } = string.Empty;
}