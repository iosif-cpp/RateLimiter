using RateLimiter.Writer.DAL.Models;
using RateLimiter.Writer.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace RateLimiter.Writer.DAL.Mappers;

[Mapper]
public static partial class RateLimitMapper
{
    public static partial RateLimitDbModel ToDbModel(this RateLimit domain);
    public static partial RateLimit? ToDomain(this RateLimitDbModel? dbModel);
}