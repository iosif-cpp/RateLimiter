using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RateLimiter.Reader.DAL.Models;

public class RateLimitDbModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("route")] public string Route { get; set; } = null!;

    [BsonElement("requests_per_minute")] public int RequestsPerMinute { get; set; }
}