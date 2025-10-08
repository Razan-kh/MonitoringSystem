using System.Threading.Tasks;
using MonitoringSystem.Shared.Models;

namespace MonitoringSystem.Shared.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync(Message message);
}