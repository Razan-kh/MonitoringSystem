using Microsoft.AspNetCore.SignalR;

using MonitoringSystem.Shared.Models;

public class AlertsHub : Hub
{
    public async Task SendAlert(AlertMessage message)
    {
        Console.WriteLine($"Alert recieved {message}");
        await Clients.All.SendAsync("ReceiveAlert", message);
    }
}