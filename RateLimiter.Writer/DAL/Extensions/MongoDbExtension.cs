using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RateLimiter.Writer.DAL.Interfaces;
using RateLimiter.Writer.DAL.Mappers;
using RateLimiter.Writer.DAL.Repositories;

namespace RateLimiter.Writer.DAL.Extensions;

public static class MongoDbExtension
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        MongoDbClassMapRegistry.RegisterClassMaps();

        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDb"));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return client.GetDatabase(settings.DatabaseName);
        });

        services.AddSingleton<IRateLimitRepository, RateLimitRepository>();

        return services;
    }
}