using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using MonitoringSystem.Shared.Interfaces;
using MonitoringSystem.Shared.Models;

namespace MonitoringSystem.Processor.Services;

public class SignalRNotifier : ISignalRNotifier
{
    private readonly HubConnection _connection;

    public SignalRNotifier(string hubUrl)
    {
        _connection = new HubConnectionBuilder()
        .WithUrl(hubUrl)
        .Build();
        _connection.StartAsync().GetAwaiter().GetResult();
    }

    public async Task SendAlertAsync(AlertMessage alert)
    {
        await _connection.InvokeAsync("SendAlert", alert);
    }
}