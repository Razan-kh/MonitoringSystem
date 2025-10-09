using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MonitoringSystem.Shared.Interfaces;
using MonitoringSystem.Processor.Services;
using MonitoringSystem.Processor.Models;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        var rabbitHost = configuration["RabbitMQ:HostName"];
        var exchangeName = configuration["RabbitMQ:ExchangeName"];
        var mongoConn = configuration["MongoDB:ConnectionString"];
        var mongoDb = configuration["MongoDB:Database"];
        var hubUrl = configuration["SignalR:HubUrl"];

        // Register services with DI
        _ = services.AddSingleton<IMessageConsumer>(provider =>
            new RabbitMqConsumer(rabbitHost, exchangeName));

        _ = services.AddSingleton<IStatisticsRepository>(provider =>
            new MongoStatisticsRepository(mongoConn, mongoDb));

        _ = services.AddSingleton<ISignalRNotifier>(provider =>
            new SignalRNotifier(hubUrl));

        // Anomaly config binding
        _ = services.Configure<AnomalyConfig>(configuration.GetSection("AnomalyConfig"));
        _ = services.AddSingleton<AnomalyDetector>();
        // Register hosted service
        _ = services.AddHostedService<ProcessorHostedService>();
    })
    .Build();

host.Run();