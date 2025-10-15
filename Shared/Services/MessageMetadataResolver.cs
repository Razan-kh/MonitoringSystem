using System.Reflection;
using Microsoft.Extensions.Configuration;
using MonitoringSystem.Shared.Models;

namespace MonitoringSystem.Shared.Services;
 
public static class MessageMetadataResolver
{
    public static (string Exchange, string Topic) Resolve<T>(IConfiguration configuration)
    {
        var attr = typeof(Message<T>).GetCustomAttribute<MessageMetadataAttribute>();
        if (attr == null)
            throw new InvalidOperationException($"No metadata defined for {typeof(T).Name}");

        // Resolve exchange and topic from configuration
        var exchange = configuration[attr.Exchange] ?? 
            throw new InvalidOperationException($"Exchange not found in configuration for key '{attr.Exchange}'");

        var topic = configuration[attr.Topic] ?? 
            throw new InvalidOperationException($"Topic not found in configuration for key '{attr.Topic}'");

        return (exchange, topic);
    }
}
