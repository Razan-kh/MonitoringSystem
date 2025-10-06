using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MonitoringSystem.Collector.Services;
using MonitoringSystem.Collector.Models;
using MonitoringSystem.Shared.Interfaces;

var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                services.Configure<CollectorConfig>(configuration.GetSection("CollectorConfig"));

                // Get hostName from configuration with fallback
                var hostName = configuration["RabbitMQ:HostName"] ?? "localhost";

                services.AddSingleton<IMessagePublisher>(provider =>
                    new RabbitMqPublisher(hostName));

                services.AddHostedService<StatisticsCollectorService>();
            })
            .Build();

host.Run();