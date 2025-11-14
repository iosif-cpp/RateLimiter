using RateLimiter.Reader.DAL.Models;
using RateLimiter.Reader.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace RateLimiter.Reader.DAL.Mappers;

[Mapper]
public static partial class RateLimitMapper
{
    public static partial RateLimitDbModel ToDbModel(this RateLimit domain);
    public static partial RateLimit? ToDomain(this RateLimitDbModel? dbModel);
}