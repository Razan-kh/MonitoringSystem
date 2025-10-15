using System.Reflection;

namespace MonitoringSystem.Shared.Models;

[MessageMetadata("ExchangeNames:Server", "Topics:ServerStatistics")]
public class Message<T>
{
    public T Content { get; init; }
}