using UserService.Api.Services;
using UserService.DAL.Interfaces;
using UserService.DAL.Repositories;
using UserService.SshConnection;
using UserService.AppLayer.Validators;

namespace UserService;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSshTunnel(builder.Configuration);

        builder.Logging.AddConsole();

        builder.Services.AddGrpc();

        builder.Services.AddSingleton<IUserRepository, UserRepository>();
        builder.Services.AddSingleton<UserService.AppLayer.Services.UserService>();

        builder.Services.AddSingleton<AppLayer.Services.UserService>();
        builder.Services.AddSingleton<UserValidator>();

        builder.Services.AddGrpc();

        var app = builder.Build();

        app.MapGrpcService<UserServiceRpc>();

        await app.RunAsync();
    }
}