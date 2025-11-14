namespace UserService.SshConnection;

using Microsoft.Extensions.DependencyInjection;

public static class SshTunnelServiceExtensions
{
    public static IServiceCollection AddSshTunnel(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SshTunnelSettings>(configuration.GetSection("Postgres").GetSection("SshTunnel"));
        services.Configure<DatabaseSettings>(configuration.GetSection("Postgres").GetSection("Database"));

        services.AddHostedService<SshTunnelHostedService>();

        return services;
    }
}