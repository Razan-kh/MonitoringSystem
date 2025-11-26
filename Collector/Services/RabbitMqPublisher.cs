using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using MonitoringSystem.Shared.Interfaces;
using MonitoringSystem.Shared.Models;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MonitoringSystem.Shared.Services;

namespace MonitoringSystem.Collector.Services;

public class RabbitMqPublisher : IMessagePublisher
{
    private readonly ConnectionFactory _factory;
    private readonly IConfiguration _configuration;

    public RabbitMqPublisher(string hostName, IConfiguration configuration)
    {
        _factory = new ConnectionFactory() { HostName = hostName };
        _configuration = configuration;
    }

    public Task PublishAsync<T>(Message<T> message)
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        var (exchange, topic) = MessageMetadataResolver.Resolve<T>(_configuration);

        channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        channel.BasicPublish(exchange, topic, null, body);

        return Task.CompletedTask;
    }
}