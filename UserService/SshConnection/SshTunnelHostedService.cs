using Microsoft.Extensions.Options;
using Renci.SshNet;

namespace UserService.SshConnection;

internal class SshTunnelHostedService : BackgroundService
{
    private readonly IOptions<SshTunnelSettings> _sshSettings;
    private readonly ILogger<SshTunnelHostedService> _logger;
    private SshClient? _client;
    private ForwardedPortLocal? _portForwarded;

    public SshTunnelHostedService(
        IOptions<SshTunnelSettings> sshSettings,
        ILogger<SshTunnelHostedService> logger)
    {
        _sshSettings = sshSettings;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = _sshSettings.Value;

        var connectionInfo = new Renci.SshNet.ConnectionInfo(
            settings.SshHost,
            settings.SshPort,
            settings.SshUsername,
            new PasswordAuthenticationMethod(settings.SshUsername, settings.SshPassword)
        );

        _client = new SshClient(connectionInfo);
        _client.Connect();

        _portForwarded = new ForwardedPortLocal(
            settings.LocalHost,
            settings.LocalPort,
            settings.RemoteHost,
            settings.RemotePort
        );

        _client.AddForwardedPort(_portForwarded);
        _portForwarded.Start();

        _logger.LogInformation("SSH-туннель поднят и работает");

        await Task.Delay(-1, stoppingToken); 

        await StopTunnelAsync();
    }

    private Task StopTunnelAsync()
    {
        try
        {
            _portForwarded?.Stop();
            _client?.Disconnect();
            _client?.Dispose();
            _logger.LogInformation("SSH-туннель остановлен");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при остановке SSH-туннеля");
        }

        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await StopTunnelAsync();
        await base.StopAsync(cancellationToken);
    }
}