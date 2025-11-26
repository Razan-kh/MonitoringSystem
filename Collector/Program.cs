using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MonitoringSystem.Collector.Services;
using MonitoringSystem.Collector.Models;
using MonitoringSystem.Shared.Interfaces;

var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                // Add support for environment variables
                _ = config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                _ = services.Configure<CollectorConfig>(configuration.GetSection("ServerStatisticsConfig"));

                // Get hostName from configuration with fallback
                var hostName = configuration["RabbitMQ:HostName"];
              //  var exchangeName = configuration["RabbitMQ:ExchangeName"];

                _ = services.AddSingleton<CpuUsageCollector>();
                _ = services.AddSingleton<MemoryUsageCollector>();

                _ = services.AddSingleton<IMessagePublisher>(provider =>
                    new RabbitMqPublisher(hostName, configuration.GetSection("RabbitMq")));

                _ = services.AddHostedService<StatisticsCollectorService>();
            })
            .Build();

host.Run();