using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using MonitoringSystem.Shared.Interfaces;
using MonitoringSystem.Shared.Models;
using System.Text.Json;

namespace MonitoringSystem.Collector.Services;

public class RabbitMqPublisher : IMessagePublisher
{
    private readonly ConnectionFactory _factory;

    public RabbitMqPublisher(string hostName, string exchange)
    {
        _factory = new ConnectionFactory() { HostName = hostName };
    }

    public Task PublishAsync<T>(Message<T> message)
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();
        
        channel.ExchangeDeclare(message.Exchange, ExchangeType.Topic, durable: true);
        
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        channel.BasicPublish(message.Exchange, message.Topic, null, body);
        return Task.CompletedTask;
    }
}