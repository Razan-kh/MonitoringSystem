using System;
using System.Threading.Tasks;
using MonitoringSystem.Shared.Interfaces;
using MonitoringSystem.Shared.Models;
using Microsoft.Extensions.Options;
using MonitoringSystem.Processor.Models;
using MonitoringSystem.Shared.Models;

namespace MonitoringSystem.Processor.Services;

public class AnomalyDetector
{
    private readonly IStatisticsRepository _repo;
    private readonly ISignalRNotifier _notifier;
    private readonly AnomalyConfig _config;

    public AnomalyDetector(
        IStatisticsRepository repo,
        ISignalRNotifier notifier,
        IOptions<AnomalyConfig> options)
    {
        _repo = repo;
        _notifier = notifier;
        _config = options.Value;
    }

    public async Task ProcessAsync(ServerStatistics current)
    {
        try
        {
            var previous = await _repo.GetPreviousAsync(current.ServerIdentifier);
            await _repo.InsertAsync(current);

            if (previous != null)
            {
                await Task.WhenAll(
                    AlertMemoryAnomalyAsync(previous, current),
                    AlertCpuAnomalyAsync(previous, current)
                );
            }
        }
        catch (Exception ex)
        {
            Console.Write($"Error processing anomaly detection for {current.ServerIdentifier}: {ex.Message}");
        }
    }

    private async Task AlertMemoryAnomalyAsync(ServerStatistics previous, ServerStatistics current)
    {
        if (current.MemoryUsage > previous.MemoryUsage * (1 + _config.MemoryUsageAnomalyThresholdPercentage))
        {
            await SendAlertAsync(
                current.ServerIdentifier,
                AnomalyType.MemoryAnomaly,
                $"Memory jumped from {previous.MemoryUsage}MB to {current.MemoryUsage}MB",
                current);
        }
    }

    private async Task AlertCpuAnomalyAsync(ServerStatistics previous, ServerStatistics current)
    {
        if (current.CpuUsage > previous.CpuUsage * (1 + _config.CpuUsageAnomalyThresholdPercentage))
        {
            await SendAlertAsync(
                current.ServerIdentifier,
                AnomalyType.CpuAnomaly,
                $"CPU jumped from {previous.CpuUsage}% to {current.CpuUsage}%",
                current);
        }
    }

    private async Task SendAlertAsync(string serverId, AnomalyType type, string description, ServerStatistics stats)
    {
        await _notifier.SendAlertAsync(new AlertMessage
        {
            ServerIdentifier = serverId,
            Type = type,
            Description = description,
            Stats = stats
        });
    }
}