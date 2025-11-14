using RateLimiter.Reader.Redis.Interfaces;
using StackExchange.Redis;

namespace RateLimiter.Reader.Redis.Services;

public class RedisReaderService : IRedisReaderService
{
    private readonly IDatabase _db;
    
    public RedisReaderService(IConnectionMultiplexer multiplexer) => _db = multiplexer.GetDatabase();

    public Task<bool> IsBannedAsync(int userId)
    {
        var banKey = $"user:{userId}ban";
        return _db.KeyExistsAsync(banKey);
    }

    public Task BanAsync(int userId)
    {
        var banKey = $"user:{userId}ban";
        return _db.StringSetAsync(banKey, 1, TimeSpan.FromMinutes(5));
    }

    public Task DeleteCounterAsync(int userId, string endpoint)
    {
        var counterKey = GetCounterKey(userId, endpoint);
        return _db.KeyDeleteAsync(counterKey);
    }

    public async Task<long> IncrementCounterAsync(int userId, string endpoint)
    {
        var counterKey = GetCounterKey(userId, endpoint);
        var value = await _db.StringIncrementAsync(counterKey).ConfigureAwait(false);

        if (value == 1)
        {
            await _db.KeyExpireAsync(counterKey, TimeSpan.FromMinutes(1)).ConfigureAwait(false);
        }
        return value;
    }

    private static string GetCounterKey(int userId, string endpoint) => $"rpm:{userId}:{endpoint}";
}


