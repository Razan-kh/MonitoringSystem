namespace MonitoringSystem.Shared.Models;

public class Message<T>
{
    public string Topic { get; init; }
    public T Content { get; init; }
    public string Exchange { get; init; }
}
