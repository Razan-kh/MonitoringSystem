﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MonitoringSystem.Shared.Interfaces;
using MonitoringSystem.Processor.Services;
using MonitoringSystem.Processor.Models;
using MonitoringSystem.Shared.Models;
using MonitoringSystem.Shared.Services;

var hostBuilder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        var rabbitHost = configuration["RabbitMQ:HostName"];
        var exchangeName = configuration["RabbitMQ:ExchangeNames:Server"];
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
    });

var host = hostBuilder.Build();

var configuration = host.Services.GetRequiredService<IConfiguration>();

using var scope = host.Services.CreateScope();
var consumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();
var detector = scope.ServiceProvider.GetRequiredService<AnomalyDetector>();

var topic = MessageMetadataResolver.Resolve<ServerStatistics>(configuration.GetSection("RabbitMQ")).Topic;

await consumer.StartAsync<ServerStatistics>(topic, async message =>
{
    if (message.Content != null)
    {
        Console.WriteLine($"Received from {message.Content.ServerIdentifier}");
        await detector.ProcessAsync(message.Content);
    }
});

Console.WriteLine("Consumer started. Press Ctrl+C to exit.");

//host.Run();
await host.RunAsync();
