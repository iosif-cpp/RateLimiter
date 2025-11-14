namespace RateLimiter.Reader.DAL.Extensions;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string RateLimitCollectionName { get; set; } = null!;
}