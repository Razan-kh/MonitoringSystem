using System.Threading.Tasks;
using MonitoringSystem.Shared.Models;

namespace MonitoringSystem.Shared.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(Message<T> message);
}