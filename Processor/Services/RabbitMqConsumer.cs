#nullable enable

using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MonitoringSystem.Shared.Interfaces;
using MonitoringSystem.Shared.Models;
using System.Text.Json;

namespace MonitoringSystem.Processor.Services;

public class RabbitMqConsumer : IMessageConsumer
{
    private readonly ConnectionFactory _factory;
    private readonly string _exchangeName;
    private IConnection? _connection;
    private IModel? _channel;
    private string? _queueName;

    public RabbitMqConsumer(string hostName, string exchangeName)
    {
        _factory = new ConnectionFactory() { HostName = hostName };
        _exchangeName = exchangeName;
    }

    public Task StartAsync<T>(string topicPattern, Func<Message<T>, Task> messageHandler)
    {
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(_exchangeName, ExchangeType.Topic, durable: true);
        _queueName = _channel.QueueDeclare().QueueName;
        _channel.QueueBind(_queueName, _exchangeName, topicPattern);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (sender, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            try
            {
                var message = JsonSerializer.Deserialize<Message<T>>(json);

                if (message != null)
                {
                    Console.WriteLine($"message received {message.Content}");
                    await messageHandler(message);
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                else
                {
                    Console.WriteLine("Failed to deserialize message or content is null");
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _channel?.Close();
        _connection?.Close();
        return Task.CompletedTask;
    }
}