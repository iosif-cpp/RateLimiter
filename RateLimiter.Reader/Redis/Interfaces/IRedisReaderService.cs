namespace RateLimiter.Reader.Redis.Interfaces;

public interface IRedisReaderService
{
    Task<bool> IsBannedAsync(int userId);
    Task BanAsync(int userId);
    Task DeleteCounterAsync(int userId, string endpoint);
    Task<long> IncrementCounterAsync(int userId, string endpoint);
}