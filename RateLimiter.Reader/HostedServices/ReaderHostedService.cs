using RateLimiter.Reader.AppLayer.Interfaces;

namespace RateLimiter.Reader.HostedServices;

public class ReaderHostedService : IHostedService
{
    private readonly IReaderService _readerService;

    public ReaderHostedService(IReaderService readerService)
    {
        _readerService = readerService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _readerService.InitializeAsync(cancellationToken);
        _readerService.StartWatchingChanges();
    }

    public Task StopAsync(CancellationToken _)
    {
        return Task.CompletedTask;
    }
}