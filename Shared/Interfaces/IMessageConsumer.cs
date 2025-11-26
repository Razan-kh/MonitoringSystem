using System;
using System.Threading.Tasks;

using MonitoringSystem.Shared.Models;

namespace MonitoringSystem.Shared.Interfaces;

public interface IMessageConsumer
{
    Task StartAsync<T>(string topicPattern, Func<Message<T>, Task> messageHandler);
    Task StopAsync();
}