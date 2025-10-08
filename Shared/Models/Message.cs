namespace MonitoringSystem.Shared.Models;

public class Message
{
    public string Topic { get; init; }
    public string Content { get; init; }
    public string Exchange { get; init; }
}
