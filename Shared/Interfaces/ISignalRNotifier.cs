using MonitoringSystem.Shared.Models;
using System.Threading.Tasks;

namespace MonitoringSystem.Shared.Interfaces;

public interface ISignalRNotifier
{
    Task SendAlertAsync(AlertMessage alert);
}