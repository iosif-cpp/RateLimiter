namespace RateLimiter.Writer.AppLayer.Validators;

public static class PossibleRoutes
{
    public static readonly HashSet<string> Routes =
    [
        "UserServiceApi/CreateUser",
        "UserServiceApi/GetUserById",
        "UserServiceApi/GetUserByName",
        "UserServiceApi/UpdateUser",
        "UserServiceApi/DeleteUser"
    ];
}