using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using MonitoringSystem.Collector.Interfaces;
using MonitoringSystem.Collector.Models;
using MonitoringSystem.Shared.Models;
using MonitoringSystem.Shared.Interfaces;
using MonitoringSystem.Collector.Exceptions;
using Microsoft.Extensions.Configuration;

namespace MonitoringSystem.Collector.Services;

public class StatisticsCollectorService : BackgroundService
{
    private readonly IMessagePublisher _publisher;
    private readonly CollectorConfig _config;
    private readonly CpuUsageCollector _cpuCollector;
    private readonly MemoryUsageCollector _memoryCollector;

    public StatisticsCollectorService(
        IMessagePublisher publisher,
        IOptions<CollectorConfig> options,
        CpuUsageCollector cpuCollector,
        MemoryUsageCollector memoryCollector)
    {
        _publisher = publisher;
        _config = options.Value;
        _cpuCollector = cpuCollector;
        _memoryCollector = memoryCollector;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var stats = Collect();
                await PublishStatisticsAsync(stats);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error collecting or publishing statistics: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(_config.SamplingIntervalSeconds), stoppingToken);
        }
    }

    private ServerStatistics Collect()
    {
        var cpuUsage = GetCpuUsage();
        var memoryUsage = GetMemoryUsage();
        return new ServerStatistics
        {
            ServerIdentifier = _config.ServerIdentifier,
            CpuUsage = cpuUsage,
            MemoryUsage = memoryUsage.UsedMB,
            AvailableMemory = memoryUsage.AvailableMB,
            Timestamp = DateTime.Now
        };
    }

    private double GetCpuUsage()
    {
        try
        {
            return _cpuCollector.GetCpuUsage();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CPU usage collection failed: {ex.Message}");
            throw;
        }
    }

    private MemoryUsage GetMemoryUsage()
    {
        try
        {
            return _memoryCollector.GetMemoryUsage();
        }
        catch (MemoryUsageException mex)
        {
            Console.WriteLine($"Memory usage collection failed: {mex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error during memory collection: {ex.Message}");
            throw;
        }
    }

    private async Task PublishStatisticsAsync(ServerStatistics stats)
    {
        var message = new Message<ServerStatistics>
        {
            Content = stats
        };
        await _publisher.PublishAsync(message);
        Console.WriteLine($"Published stats for {_config.ServerIdentifier} at {DateTime.Now}");
    }
}