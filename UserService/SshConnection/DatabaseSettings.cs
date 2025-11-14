namespace UserService.SshConnection;

public class DatabaseSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;

    public string GetConnectionString() =>
        $"Host={Host};Port={Port};Username={Username};Password={Password};Database={DatabaseName}";
}