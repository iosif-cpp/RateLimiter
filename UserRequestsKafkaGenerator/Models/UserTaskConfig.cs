using UserRequestsKafkaGenerator.Common;

namespace UserRequestsKafkaGenerator.Models;

public class UserTaskConfig
{
    public int UserId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public int Rpm { get; set; }
    public bool IsActive { get; set; } = true;
    public CancellationTokenSource? CancellationTokenSource { get; set; }
    public string TaskId => TaskIdCreator.GetTaskId(UserId, Endpoint);
}