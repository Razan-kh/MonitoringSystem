using MonitoringSystem.Shared.Models;
using System.Threading.Tasks;

namespace MonitoringSystem.Shared.Interfaces;

public interface IStatisticsRepository
{
    Task InsertAsync(ServerStatistics stats);
    Task<ServerStatistics> GetPreviousAsync(string serverIdentifier);
}