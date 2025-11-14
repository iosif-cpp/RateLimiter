using RateLimiter.Reader.DAL.Models;
using RateLimiter.Reader.Domain.Entities;

namespace RateLimiter.Reader.DAL.Interfaces;

public interface IRepository
{
    IAsyncEnumerable<IReadOnlyList<RateLimit>> ReadAllInBatchesAsync();
    IAsyncEnumerable<RateLimitChange> WatchChangesAsync();
}