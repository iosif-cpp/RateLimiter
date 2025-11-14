using FluentValidation;
using RateLimiter.Writer.AppLayer.Interfaces;
using RateLimiter.Writer.DAL.Interfaces;
using RateLimiter.Writer.Domain.Entities;

namespace RateLimiter.Writer.AppLayer.Services;

public class WriterService : IWriterService
{
    private readonly IRateLimitRepository _repository;
    private readonly IValidator<RateLimit> _validator;

    public WriterService(IRateLimitRepository repository, IValidator<RateLimit> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<RateLimit?> CreateRateLimitAsync(RateLimit limit, CancellationToken ct)
    {
        var result = _validator.Validate(limit);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        return await _repository.CreateAsync(limit, ct);
    }

    public Task<RateLimit?> GetRateLimitByRouteAsync(string route, CancellationToken ct)
    {
        return _repository.GetByRouteAsync(route, ct);
    }

    public async Task<bool> UpdateRateLimitAsync(RateLimit rateLimit, CancellationToken ct)
    {
        var result = _validator.Validate(rateLimit);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        var updated = await _repository.UpdateAsync(rateLimit, ct);
        return updated != null;
    }

    public Task<bool> DeleteRateLimitAsync(string route, CancellationToken ct)
    {
        return _repository.DeleteByRouteAsync(route, ct);
    }
}