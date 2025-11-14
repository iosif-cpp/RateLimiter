namespace RateLimiter.Reader.AppLayer.Interfaces;

public interface IReaderService
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    void StartWatchingChanges();
    IReadOnlyCollection<Domain.Entities.RateLimit> GetAllLimitsSnapshot();
}