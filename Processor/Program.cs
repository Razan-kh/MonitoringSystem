using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using ServerMonitoring.Shared.Interfaces;
using ServerMonitoring.Processor.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        const string defaultRabbitHost = "localhost";
        const string defaultMongoConn = "mongodb://localhost:27017";
        const string defaultMongoDb = "ServerMonitoringDb";
        const string defaultHubUrl = "http://localhost:5000/alertsHub";

        // Read configuration with fallback
        var rabbitHost = configuration["RabbitMQ:HostName"] ?? defaultRabbitHost;
        var mongoConn = configuration["MongoDB:ConnectionString"] ?? defaultMongoConn;
        var mongoDb = configuration["MongoDB:Database"] ?? defaultMongoDb;
        var hubUrl = configuration["SignalR:HubUrl"] ?? defaultHubUrl;

        // Register services with DI
        services.AddSingleton<IMessageConsumer>(provider =>
            new RabbitMqConsumer(rabbitHost));

        services.AddSingleton<IStatisticsRepository>(provider =>
            new MongoStatisticsRepository(mongoConn, mongoDb));

        services.AddSingleton<ISignalRNotifier>(provider =>
            new SignalRNotifier(hubUrl));

        // Anomaly config binding
        services.Configure<AnomalyConfig>(configuration.GetSection("AnomalyConfig"));

        // Register hosted service
        services.AddHostedService<ProcessorHostedService>();
    })
    .Build();

host.Run();