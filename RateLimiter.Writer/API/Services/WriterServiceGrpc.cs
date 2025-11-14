using Grpc.Core;
using RateLimiter.Writer.API.Mappers;
using RateLimiter.Writer.AppLayer.Interfaces;

namespace RateLimiter.Writer.API.Services;

public class WriterServiceGrpc : RateLimiterReaderServiceApi.RateLimiterReaderServiceApiBase
{
    private readonly IWriterService _writerService;

    public WriterServiceGrpc(IWriterService writerService)
    {
        _writerService = writerService;
    }

    public override async Task<CreateLimitResponse> CreateLimit(CreateLimitRequest request, ServerCallContext context)
    {
        var limit = request.ToEntity();
        var createdLimit = await _writerService.CreateRateLimitAsync(limit, context.CancellationToken);

        if (createdLimit == null)
        {
            return new CreateLimitResponse
            {
                Success = false,
                ErrorMessage = "Failed to create limit"
            };
        }

        return new CreateLimitResponse
        {
            Success = true,
            RateLimit = createdLimit.ToDto()
        };
    }

    public override async Task<GetLimitByRouteResponse> GetLimitByRoute(GetLimitByRouteRequest request,
        ServerCallContext context)
    {
        var limit = await _writerService.GetRateLimitByRouteAsync(request.Route, context.CancellationToken);

        if (limit == null)
        {
            return new GetLimitByRouteResponse()
            {
                Found = false
            };
        }

        return new GetLimitByRouteResponse()
        {
            Found = true,
            RateLimit = limit.ToDto()
        };
    }

    public override async Task<UpdateLimitResponse> UpdateLimit(UpdateLimitRequest request, ServerCallContext context)
    {
        var existingLimit = await _writerService.GetRateLimitByRouteAsync(request.Route, context.CancellationToken);
        if (existingLimit == null)
        {
            return new UpdateLimitResponse()
            {
                Success = false,
                ErrorMessage = $"Limit with route {request.Route} not found"
            };
        }

        request.UpdateFromRequest(existingLimit);
        var success = await _writerService.UpdateRateLimitAsync(existingLimit, context.CancellationToken);

        return new UpdateLimitResponse()
        {
            Success = success,
            ErrorMessage = success ? string.Empty : "Failed to update limit"
        };
    }

    public override async Task<DeleteLimitResponse> DeleteLimit(DeleteLimitRequest request, ServerCallContext context)
    {
        var success = await _writerService.DeleteRateLimitAsync(request.Route, context.CancellationToken);

        return new DeleteLimitResponse()
        {
            Success = success
        };
    }
}