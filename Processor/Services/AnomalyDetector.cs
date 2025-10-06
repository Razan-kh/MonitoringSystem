using System.Threading.Tasks;
using ServerMonitoring.Shared.Interfaces;
using ServerMonitoring.Shared.Models;
using Microsoft.Extensions.Options;
using MonitoringSystem.Processor.Models;

namespace ServerMonitoring.Processor.Services;

public class AnomalyDetector
{
    private readonly IStatisticsRepository _repo;
    private readonly ISignalRNotifier _notifier;
    private readonly AnomalyConfig _config;

    public AnomalyDetector(IStatisticsRepository repo, ISignalRNotifier notifier, IOptions<AnomalyConfig> options)
    {
        _repo = repo;
        _notifier = notifier;
        _config = options.Value;
    }

    public async Task ProcessAsync(ServerStatistics current)
    {
        var prev = await _repo.GetPreviousAsync(current.ServerIdentifier);

        await _repo.InsertAsync(current);

        if (prev != null)
        {
            if (current.MemoryUsage > prev.MemoryUsage * (1 + _config.MemoryUsageAnomalyThresholdPercentage))
            {
                await _notifier.SendAlertAsync(
                    new AlertMessage
                    {
                        ServerIdentifier = current.ServerIdentifier,
                        Type = "MemoryAnomaly",
                        Description = $"Memory jumped from {prev.MemoryUsage}MB to {current.MemoryUsage}MB",
                        Stats = current
                    });
            }

            if (current.CpuUsage > prev.CpuUsage * (1 + _config.CpuUsageAnomalyThresholdPercentage))
            {
                await _notifier.SendAlertAsync(
                    new AlertMessage
                    {
                        ServerIdentifier = current.ServerIdentifier,
                        Type = "CpuAnomaly",
                        Description = $"CPU jumped from {prev.CpuUsage}% to {current.CpuUsage}%",
                        Stats = current
                    });
            }
        }
    }
}