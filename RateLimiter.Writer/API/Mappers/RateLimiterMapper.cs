using RateLimiter.Writer.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace RateLimiter.Writer.API.Mappers;

[Mapper]
public static partial class RateLimiterMapper
{
    public static partial RateLimitDto ToDto(this RateLimit rateLimiter);

    public static partial RateLimit ToEntity(this CreateLimitRequest request);

    public static partial void UpdateFromRequest(this UpdateLimitRequest request, RateLimit rateLimit);
}