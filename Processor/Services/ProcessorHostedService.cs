using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using MonitoringSystem.Shared.Models;
using MonitoringSystem.Shared.Interfaces;
using Microsoft.Extensions.Options;
using MonitoringSystem.Processor.Models;

namespace MonitoringSystem.Processor.Services;

public class ProcessorHostedService : BackgroundService
{
    private readonly IMessageConsumer _consumer;
    private readonly AnomalyDetector _detector;

    public ProcessorHostedService(IMessageConsumer consumer, AnomalyDetector detector)
    {
        _consumer = consumer;
        _detector = detector;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.StartAsync<ServerStatistics>("ServerStatistics.*", async message =>
        {
            if (message.Content != null)
            {
                await _detector.ProcessAsync(message.Content);
            }
        });

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
        await _consumer.StopAsync();
    }
}