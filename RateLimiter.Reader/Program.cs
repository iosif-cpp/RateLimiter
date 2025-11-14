using FluentValidation;
using RateLimiter.Reader.AppLayer.Interfaces;
using RateLimiter.Reader.AppLayer.Services;
using RateLimiter.Reader.Controllers;
using RateLimiter.Reader.HostedServices;
using RateLimiter.Reader.DAL.Extensions;
using RateLimiter.Reader.Kafka;
using RateLimiter.Reader.Kafka.Services;
using RateLimiter.Reader.Redis.Interfaces;
using RateLimiter.Reader.Redis.Services;
using RateLimiter.Writer.AppLayer.Interfaces;
using RateLimiter.Writer.AppLayer.Services;
using RateLimiter.Writer.AppLayer.Validators;
using RateLimiter.Writer.DAL.Interfaces;
using RateLimiter.Writer.DAL.Mappers;
using RateLimiter.Writer.DAL.Repositories;
using RateLimiter.Writer.Domain.Entities;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddGrpc();
builder.Services.AddMongoDb(builder.Configuration);
builder.Services.AddSingleton<IReaderService, ReaderService>();
builder.Services.AddHostedService<ReaderHostedService>();
builder.Services.AddHostedService<KafkaConsumerService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect("localhost:6379"));
builder.Services.AddSingleton<IRedisReaderService, RedisReaderService>();
builder.Services.AddSingleton<IValidator<RateLimit>, RateLimitValidator>();
builder.Services.Configure<RateLimiter.Writer.DAL.Extensions.MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));
builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection("Kafka"));
MongoDbClassMapRegistry.RegisterClassMaps();
builder.Services.AddSingleton<IRateLimitRepository, RateLimitRepository>();
builder.Services.AddSingleton<IWriterService, WriterService>();
var app = builder.Build();

app.MapGrpcService<ReaderController>();

await app.RunAsync("http://*:5000");