using System.Collections.Concurrent;
using RateLimiter.Reader.AppLayer.Interfaces;
using RateLimiter.Reader.DAL.Interfaces;
using RateLimiter.Reader.DAL.Models;
using RateLimiter.Reader.Domain.Entities;

namespace RateLimiter.Reader.AppLayer.Services;

public class ReaderService : IReaderService
{
    private readonly IRepository _repository;
    private readonly ConcurrentDictionary<string, RateLimit> _limits = new();
    private Task? _watchingTask;
    private bool _isInitialized;

    public ReaderService(IRepository repository)
    {
        _repository = repository;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized) return;

        await foreach (var batch in _repository.ReadAllInBatchesAsync().WithCancellation(cancellationToken))
        {
            foreach (var limit in batch)
            {
                _limits[limit.Route] = limit;
            }
        }

        _isInitialized = true;
    }

    public void StartWatchingChanges()
    {
        if (_watchingTask != null || !_isInitialized) return;

        _watchingTask = Task.Run(async () =>
        {
            await foreach (var change in _repository.WatchChangesAsync())
            {
                switch (change.Change)
                {
                    case PossibleChanges.Inserted:
                    case PossibleChanges.Updated:
                        if (change.NewRateLimit != null)
                        {
                            _limits[change.Route] = change.NewRateLimit;
                        }

                        break;
                    case PossibleChanges.Deleted:
                        _limits.TryRemove(change.Route, out _);
                        break;
                }
            }
        });
    }

    public IReadOnlyCollection<RateLimit> GetAllLimitsSnapshot()
    {
        return _limits.Values.ToArray();
    }
}