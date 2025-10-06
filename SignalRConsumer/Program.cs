using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR.Client;

namespace MonitoringSystem.SignalRConsumer;

public class Program
{
    private const string DefaultHubUrl = "http://localhost:5000/alertsHub";

    public static async Task Main(string[] args)
    {
        try
        {
            // Build configuration
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Get the hub URL from configuration
            var hubUrl = config.GetValue<string>("SignalR:HubUrl") ?? DefaultHubUrl;

            Console.WriteLine($"Connecting to SignalR hub: {hubUrl}");

            // Build the connection
            var connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            // Handle incoming alerts
            connection.On<object>("ReceiveAlert", (payload) =>
            {
                Console.WriteLine($"[ALERT] {DateTime.Now:HH:mm:ss} - {System.Text.Json.JsonSerializer.Serialize(payload)}");
            });

            // Handle connection events
            connection.Closed += async (error) =>
            {
                Console.WriteLine($"Connection closed: {error?.Message}");
                await Task.Delay(5000);
                await connection.StartAsync();
            };

            connection.Reconnected += (connectionId) =>
            {
                Console.WriteLine("Reconnected to SignalR hub");
                return Task.CompletedTask;
            };

            // Start the connection
            await connection.StartAsync();
            Console.WriteLine("Connected to SignalR hub. Listening for alerts...");
            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            // Stop the connection
            await connection.StopAsync();
            Console.WriteLine("Disconnected from SignalR hub.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
}
}
