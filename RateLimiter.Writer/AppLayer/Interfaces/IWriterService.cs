using RateLimiter.Writer.Domain.Entities;

namespace RateLimiter.Writer.AppLayer.Interfaces;

public interface IWriterService
{
    Task<RateLimit?> CreateRateLimitAsync(RateLimit limit, CancellationToken cancellationToken = default);
    Task<RateLimit?> GetRateLimitByRouteAsync(string route, CancellationToken cancellationToken = default);
    Task<bool> UpdateRateLimitAsync(RateLimit rateLimit, CancellationToken cancellationToken = default);
    Task<bool> DeleteRateLimitAsync(string route, CancellationToken cancellationToken = default);
}