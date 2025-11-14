using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RateLimiter.Reader.DAL.Extensions;
using RateLimiter.Reader.DAL.Interfaces;
using RateLimiter.Reader.DAL.Mappers;
using RateLimiter.Reader.DAL.Models;
using RateLimiter.Reader.Domain.Entities;

namespace RateLimiter.Reader.DAL.Repositories;

public class Repository : IRepository
{
    private const int BatchSize = 1000;
    private readonly IMongoCollection<RateLimitDbModel> _collection;

    public Repository(IMongoDatabase database, IOptions<MongoDbSettings> mongoDbSettings)
    {
        var collectionName = mongoDbSettings.Value.RateLimitCollectionName;
        _collection = database.GetCollection<RateLimitDbModel>(collectionName);
    }

    public async IAsyncEnumerable<IReadOnlyList<RateLimit>> ReadAllInBatchesAsync()
    {
        var buffer = new List<RateLimit>(BatchSize);

        var findOptions = new FindOptions<RateLimitDbModel>
        {
            BatchSize = BatchSize
        };

        using var cursor = await _collection.FindAsync(FilterDefinition<RateLimitDbModel>.Empty, findOptions);

        while (await cursor.MoveNextAsync())
        {
            foreach (var doc in cursor.Current)
            {
                var domain = doc.ToDomain();
                if (domain != null)
                {
                    buffer.Add(domain);
                    if (buffer.Count == BatchSize)
                    {
                        yield return buffer.ToArray();
                        buffer = new List<RateLimit>(BatchSize);
                    }
                }
            }
        }

        if (buffer.Count > 0)
        {
            yield return buffer.ToArray();
        }
    }

    public async IAsyncEnumerable<RateLimitChange> WatchChangesAsync()
    {
        var filter = Builders<ChangeStreamDocument<RateLimitDbModel>>.Filter.In(
            x => x.OperationType,
            new[]
            {
                ChangeStreamOperationType.Insert,
                ChangeStreamOperationType.Update,
                ChangeStreamOperationType.Delete
            });

        var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<RateLimitDbModel>>()
            .Match(filter);

        var options = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.UpdateLookup,
            FullDocumentBeforeChange = ChangeStreamFullDocumentBeforeChangeOption.Required
        };

        using var stream = await _collection.WatchAsync(pipeline, options);

        await foreach (var change in stream.ToAsyncEnumerable())
        {
            switch (change.OperationType)
            {
                case ChangeStreamOperationType.Insert:
                {
                    var doc = change.FullDocument;
                    var domain = doc.ToDomain();
                    yield return new RateLimitChange(PossibleChanges.Inserted, doc.Route, domain);
                    break;
                }

                case ChangeStreamOperationType.Update:
                {
                    var doc = change.FullDocument;
                    var domain = doc.ToDomain();
                    yield return new RateLimitChange(PossibleChanges.Updated, doc.Route, domain);
                    break;
                }

                case ChangeStreamOperationType.Delete:
                {
                    var route = change.FullDocumentBeforeChange?.Route;
                    route ??= string.Empty;
                    yield return new RateLimitChange(PossibleChanges.Deleted, route, null);
                    break;
                }
            }
        }
    }
}