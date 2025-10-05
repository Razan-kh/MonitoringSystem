using System.Threading.Tasks;

namespace MonitoringSystem.Shared.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync(string topic, string payload, string exchange);
}