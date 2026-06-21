using System.Text;
using System.Text.Json;
using Aruje.Application.Interfaces.Messaging;
using Aruje.Application.Messaging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Aruje.Infrastructure.Messaging;

public class RabbitMqMessagePublisher : IMessagePublisher
{
    private readonly IConfiguration _configuration;

    public RabbitMqMessagePublisher(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task PublishSensorReadingCreatedAsync(
        SensorReadingCreatedMessage message,
        CancellationToken cancellationToken = default)
    {
        var hostName = _configuration["RabbitMq:HostName"] ?? "localhost";
        var userName = _configuration["RabbitMq:UserName"] ?? "guest";
        var password = _configuration["RabbitMq:Password"] ?? "guest";
        var queueName = _configuration["RabbitMq:QueueName"] ?? "sensor-reading-created";

        var portValue = _configuration["RabbitMq:Port"];
        var port = int.TryParse(portValue, out var parsedPort) ? parsedPort : 5672;

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = userName,
            Password = password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(
            exchange: string.Empty,
            routingKey: queueName,
            basicProperties: properties,
            body: body
        );

        return Task.CompletedTask;
    }
}