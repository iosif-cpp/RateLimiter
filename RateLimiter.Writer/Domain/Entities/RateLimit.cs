namespace RateLimiter.Writer.Domain.Entities;

public class RateLimit
{
    public string Route { get; set; } = string.Empty;
    public int RequestsPerMinute { get; set; }
}