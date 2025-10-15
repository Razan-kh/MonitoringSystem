using Microsoft.AspNetCore.SignalR;
using MonitoringSystem.Shared.Models;

namespace MonitoringSystem.SignalRHub;

public class AlertsHub : Hub
{
    public async Task SendAlert(AlertMessage message)
    {
        Console.WriteLine($"Alert recieved from {message.ServerIdentifier} of type {message.Type} {message.Description} at {message.Timestamp}");
        await Clients.All.SendAsync("ReceiveAlert", message);
    }
}