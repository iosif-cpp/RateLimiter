namespace UserService.SshConnection;

public class SshTunnelSettings
{
    public string SshHost { get; set; } = string.Empty;
    public int SshPort { get; set; }
    public string SshUsername { get; set; } = string.Empty;
    public string SshPassword { get; set; } = string.Empty;
    public string LocalHost { get; set; } = string.Empty;
    public uint LocalPort { get; set; }
    public string RemoteHost { get; set; } = string.Empty;
    public uint RemotePort { get; set; }
}