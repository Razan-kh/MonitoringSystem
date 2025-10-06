using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using ServerMonitoring.Shared.Models;
using ServerMonitoring.Shared.Interfaces;
using Microsoft.Extensions.Options;

namespace ServerMonitoring.Processor.Services;

public class ProcessorHostedService : BackgroundService
{
    private readonly IMessageConsumer _consumer;
    private readonly AnomalyDetector _detector;

    public ProcessorHostedService(IMessageConsumer consumer, IStatisticsRepository repo, ISignalRNotifier notifier, IOptions<AnomalyConfig> opts)
    {
        _consumer = consumer;
        _detector = new AnomalyDetector(repo, notifier, opts);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.StartAsync("ServerStatistics.*", async json =>
        {
            var stats = JsonConvert.DeserializeObject<ServerStatistics>(json);
            if (stats != null)
            {
                await _detector.ProcessAsync(stats);
            }
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
        await _consumer.StopAsync();
    }
}