using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace MonitoringSystem.SignalRConsumer;

public class SignalRSubscriber
{
    private readonly string _hubUrl;
    private HubConnection _connection;

    public SignalRSubscriber(string hubUrl)
    {
        _hubUrl = hubUrl;
    }

    public async Task StartAsync()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(_hubUrl)
            .WithAutomaticReconnect()
            .Build();

        // Subscribe to alerts
        _connection.On<object>("ReceiveAlert", HandleAlert);

        // Handle connection events
        _connection.Closed += async (error) =>
        {
            Console.WriteLine($"Connection closed: {error?.Message}");
            await Task.Delay(5000);
            await _connection.StartAsync();
        };

        _connection.Reconnected += (connectionId) =>
        {
            Console.WriteLine("Reconnected to SignalR hub");
            return Task.CompletedTask;
        };

        await _connection.StartAsync();
        Console.WriteLine("Connected to SignalR hub. Listening for alerts...");
    }

    private void HandleAlert(object payload)
    {
        Console.WriteLine($"[ALERT] {DateTime.Now:HH:mm:ss} - {System.Text.Json.JsonSerializer.Serialize(payload)}");
    }

    public async Task StopAsync()
    {
        if (_connection != null)
        {
            await _connection.StopAsync();
            Console.WriteLine("Disconnected from SignalR hub.");
        }
    }
}