using FluentValidation;
using RateLimiter.Writer.API;
using RateLimiter.Writer.API.Services;
using RateLimiter.Writer.AppLayer.Interfaces;
using RateLimiter.Writer.AppLayer.Services;
using RateLimiter.Writer.AppLayer.Validators;
using RateLimiter.Writer.DAL.Extensions;
using RateLimiter.Writer.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMongoDb(builder.Configuration);

builder.Services.AddSingleton<ExceptionHandlingInterceptor>();
builder.Services.AddGrpc(options => { options.Interceptors.Add<ExceptionHandlingInterceptor>(); });

builder.Services.AddSingleton<IValidator<RateLimit>, RateLimitValidator>();
builder.Services.AddSingleton<IWriterService, WriterService>();
builder.Services.AddSingleton<WriterServiceGrpc>();

var app = builder.Build();

app.MapGrpcService<WriterServiceGrpc>();

await app.RunAsync("http://*:5001");