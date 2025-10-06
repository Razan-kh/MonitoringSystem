using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using MonitoringSystem.Shared.Models;
using MonitoringSystem.Shared.Interfaces;
using MonitoringSystem.Collector.Models;

namespace MonitoringSystem.Collector.Services;

public class StatisticsCollectorService : BackgroundService
{
    private readonly IMessagePublisher _publisher;
    private readonly CollectorConfig _config;
    private readonly CpuUsageCollector _cpuCollector;
    private readonly MemoryUsageCollector _memoryCollector;

    public StatisticsCollectorService(IMessagePublisher publisher, IOptions<CollectorConfig> options, CpuUsageCollector cpuCollector,
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
                var topic = $"ServerStatistics.{_config.ServerIdentifier}";
                var payload = JsonConvert.SerializeObject(stats);
                var exchange = "ServerStatsExchange";
                await _publisher.PublishAsync(topic, payload, exchange);
                Console.WriteLine($"Published stats for {_config.ServerIdentifier} at {DateTime.UtcNow}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error collecting statistics: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(_config.SamplingIntervalSeconds), stoppingToken);
        }
    }

    private ServerStatistics Collect()
    {
        var cpu = _cpuCollector.GetCpuUsage();
        var (used, available) = _memoryCollector.GetMemoryUsage();

        return new ServerStatistics
        {
            ServerIdentifier = _config.ServerIdentifier,
            MemoryUsage = used,
            AvailableMemory = available,
            CpuUsage = cpu,
            Timestamp = DateTime.UtcNow
        };
    }
}