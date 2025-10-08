using System;
using System.Threading.Tasks;

namespace MonitoringSystem.Shared.Interfaces;

public interface IMessageConsumer
{
    Task StartAsync(string topicPattern, Func<string, Task> messageHandler);
    Task StopAsync();
}