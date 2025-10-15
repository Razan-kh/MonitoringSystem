namespace MonitoringSystem.Shared.Models;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class MessageMetadataAttribute : Attribute
{
    public string Exchange { get; }
    public string Topic { get; }

    public MessageMetadataAttribute(string exchange, string topic)
    {
        Exchange = exchange;
        Topic = topic;
    }
}