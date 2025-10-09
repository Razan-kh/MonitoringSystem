using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR.Client;

namespace MonitoringSystem.SignalRConsumer;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var hubUrl = config.GetValue<string>("SignalR:HubUrl");

            var subscriber = new SignalRSubscriber(hubUrl);
            await subscriber.StartAsync();

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            await subscriber.StopAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}