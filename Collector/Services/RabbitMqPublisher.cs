using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using MonitoringSystem.Shared.Interfaces;
using MonitoringSystem.Shared.Models;

namespace MonitoringSystem.Collector.Services;

public class RabbitMqPublisher : IMessagePublisher
{
    private readonly ConnectionFactory _factory;

    public RabbitMqPublisher(string hostName, string exchange)
    {
        _factory = new ConnectionFactory() { HostName = hostName };
    }

    public Task PublishAsync(Message message)
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();
        var body = Encoding.UTF8.GetBytes(message.Content);

        channel.BasicPublish(message.Exchange, message.Topic, null, body);

        return Task.CompletedTask;
    }
}