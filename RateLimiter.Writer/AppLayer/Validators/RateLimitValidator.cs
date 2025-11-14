using RateLimiter.Writer.Domain.Entities;
using FluentValidation;

namespace RateLimiter.Writer.AppLayer.Validators;

public class RateLimitValidator : AbstractValidator<RateLimit>
{
    public RateLimitValidator()
    {
        RuleFor(x => x.Route)
            .NotEmpty().WithMessage("Route cannot be empty")
            .Must(route => PossibleRoutes.Routes.Contains(route)).WithMessage("Route is not possible");

        RuleFor(x => x.RequestsPerMinute)
            .GreaterThanOrEqualTo(0).WithMessage("Requests per minute must be non-negative");
    }
}