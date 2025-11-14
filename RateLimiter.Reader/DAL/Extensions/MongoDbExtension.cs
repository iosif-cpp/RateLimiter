using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RateLimiter.Reader.DAL.Interfaces;
using RateLimiter.Reader.DAL.Repositories;

namespace RateLimiter.Reader.DAL.Extensions;

public static class MongoDbExtension
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
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

        services.AddSingleton<IRepository, Repository>();

        return services;
    }
}