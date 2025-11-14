namespace RateLimiter.Writer.DAL.Models;

public class RateLimitDbModel
{
    public string Route { get; set; } = string.Empty;
    public int RequestsPerMinute { get; set; }
}