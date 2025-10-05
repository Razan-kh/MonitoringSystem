using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using MonitoringSystem.Shared.Interfaces;

namespace MonitoringSystem.Collector.Services;

public class RabbitMqPublisher : IMessagePublisher
{
    private readonly ConnectionFactory _factory;
    private readonly string _exchange;

    public RabbitMqPublisher(string hostName)
    {
        _factory = new ConnectionFactory() { HostName = hostName };
    }

    public Task PublishAsync(string topic, string message, string exchange = "PublishAsync")
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(this._exchange, ExchangeType.Topic, durable: true);

        var body = Encoding.UTF8.GetBytes(message);
        channel.BasicPublish(exchange, topic, null, body);

        return Task.CompletedTask;
    }
}