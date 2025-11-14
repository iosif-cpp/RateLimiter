using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RateLimiter.Writer.DAL.Extensions;
using RateLimiter.Writer.DAL.Interfaces;
using RateLimiter.Writer.DAL.Mappers;
using RateLimiter.Writer.DAL.Models;
using RateLimiter.Writer.Domain.Entities;

namespace RateLimiter.Writer.DAL.Repositories;

public class RateLimitRepository : IRateLimitRepository
{
    private readonly IMongoCollection<RateLimitDbModel> _collection;

    public RateLimitRepository(IMongoDatabase database, IOptions<MongoDbSettings> mongoDbSettings)
    {
        var collectionName = mongoDbSettings.Value.RateLimitCollectionName;
        _collection = database.GetCollection<RateLimitDbModel>(collectionName);
    }

    public async Task<RateLimit?> GetByRouteAsync(string route, CancellationToken ct)
    {
        var filter = Builders<RateLimitDbModel>.Filter.Eq(x => x.Route, route);
        var dbModel = await _collection.Find(filter).FirstOrDefaultAsync(ct);

        return dbModel?.ToDomain();
    }

    public async Task<RateLimit?> CreateAsync(RateLimit rateLimit, CancellationToken ct)
    {
        var dbModel = rateLimit.ToDbModel();
        await _collection.InsertOneAsync(dbModel, cancellationToken: ct);

        return dbModel.ToDomain();
    }

    public async Task<RateLimit?> UpdateAsync(RateLimit rateLimit, CancellationToken ct)
    {
        var filter = Builders<RateLimitDbModel>.Filter.Eq(x => x.Route, rateLimit.Route);
        var update = Builders<RateLimitDbModel>.Update
            .Set(x => x.RequestsPerMinute, rateLimit.RequestsPerMinute);

        var updatedDbModel = await _collection.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<RateLimitDbModel> { ReturnDocument = ReturnDocument.After },
            ct);

        return updatedDbModel?.ToDomain();
    }

    public async Task<bool> DeleteByRouteAsync(string route, CancellationToken ct)
    {
        var filter = Builders<RateLimitDbModel>.Filter.Eq(x => x.Route, route);
        var result = await _collection.DeleteOneAsync(filter, ct);

        return result.DeletedCount > 0;
    }
}