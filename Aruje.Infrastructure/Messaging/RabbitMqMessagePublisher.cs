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

        var exchangeName = _configuration["RabbitMq:ExchangeName"] ?? "aruje.sensor-readings";
        var queueName = _configuration["RabbitMq:QueueName"] ?? "sensor-reading-created";
        var routingKey = _configuration["RabbitMq:RoutingKey"] ?? "sensor-reading.created";

        var deadLetterExchangeName = _configuration["RabbitMq:DeadLetterExchangeName"] ?? "aruje.sensor-readings.dlx";
        var deadLetterQueueName = _configuration["RabbitMq:DeadLetterQueueName"] ?? "sensor-reading-created.dlq";
        var deadLetterRoutingKey = _configuration["RabbitMq:DeadLetterRoutingKey"] ?? "sensor-reading.created.dead";

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

        channel.ExchangeDeclare(
            exchange: exchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );

        channel.ExchangeDeclare(
            exchange: deadLetterExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );

        channel.QueueDeclare(
            queue: deadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        channel.QueueBind(
            queue: deadLetterQueueName,
            exchange: deadLetterExchangeName,
            routingKey: deadLetterRoutingKey
        );

        var queueArguments = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = deadLetterExchangeName,
            ["x-dead-letter-routing-key"] = deadLetterRoutingKey
        };

        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments
        );

        channel.QueueBind(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";

        channel.BasicPublish(
            exchange: exchangeName,
            routingKey: routingKey,
            basicProperties: properties,
            body: body
        );

        return Task.CompletedTask;
    }
}