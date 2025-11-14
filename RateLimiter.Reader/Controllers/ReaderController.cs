using Grpc.Core;
using RateLimiter.Reader.AppLayer.Interfaces;

namespace RateLimiter.Reader.Controllers;

public class ReaderController : Reader.ReaderBase
{
    private readonly IReaderService _readerService;

    public ReaderController(IReaderService readerService)
    {
        _readerService = readerService;
    }

    public override Task<GetAllLimitsResponse> GetAllLimits(GetAllLimitsRequest request, ServerCallContext context)
    {
        var items = _readerService.GetAllLimitsSnapshot()
            .Select(l => new RateLimitItem
            {
                Route = l.Route,
                RequestsPerMinute = l.RequestsPerMinute
            });

        var response = new GetAllLimitsResponse();
        response.Items.AddRange(items);
        return Task.FromResult(response);
    }
}