using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;
using MongoDB.Driver;

namespace RateLimiter.Writer.API;

public class ExceptionHandlingInterceptor : Interceptor
{
    private readonly ILogger<ExceptionHandlingInterceptor> _logger;

    public ExceptionHandlingInterceptor(ILogger<ExceptionHandlingInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning(ex, "Duplicate key error in gRPC call {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.AlreadyExists, "Rate limit for this route already exists"));
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error in gRPC call {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argument error in gRPC call {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal error in gRPC call {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }
}