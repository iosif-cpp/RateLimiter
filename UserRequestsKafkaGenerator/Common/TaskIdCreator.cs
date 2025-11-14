namespace UserRequestsKafkaGenerator.Common;

public static class TaskIdCreator
{
    public static string GetTaskId(int userId, string endpoint)
    {
        return $"{userId}-{endpoint}";
    }
}