using MongoDB.Bson.Serialization;
using RateLimiter.Writer.DAL.Models;

namespace RateLimiter.Writer.DAL.Mappers;

public static class MongoDbClassMapRegistry
{
    public static void RegisterClassMaps()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(RateLimitDbModel)))
            return;

        BsonClassMap.RegisterClassMap<RateLimitDbModel>(cm =>
        {
            cm.MapProperty(x => x.Route)
                .SetElementName("route");

            cm.MapProperty(x => x.RequestsPerMinute)
                .SetElementName("requests_per_minute");
        });
    }
}