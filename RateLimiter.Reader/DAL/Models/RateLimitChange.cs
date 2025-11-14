using RateLimiter.Reader.Domain.Entities;

namespace RateLimiter.Reader.DAL.Models;

public sealed record RateLimitChange(
    PossibleChanges Change,
    string Route,
    RateLimit? NewRateLimit
);