using RateLimiter.Writer.Domain.Entities;

namespace RateLimiter.Writer.DAL.Interfaces;

public interface IRateLimitRepository
{
    Task<RateLimit?> GetByRouteAsync(string route, CancellationToken ct = default);
    Task<RateLimit?> CreateAsync(RateLimit rateLimit, CancellationToken ct = default);
    Task<RateLimit?> UpdateAsync(RateLimit rateLimit, CancellationToken ct = default);
    Task<bool> DeleteByRouteAsync(string route, CancellationToken ct = default);
}