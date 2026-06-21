using System.Text;
using System.Text.Json;
using Aruje.Application.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Aruje.Worker.Consumers;

public class SensorReadingCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SensorReadingCreatedConsumer> _logger;

    private IConnection? _connection;
    private IModel? _channel;

    public SensorReadingCreatedConsumer(
        IConfiguration configuration,
        ILogger<SensorReadingCreatedConsumer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        _channel.BasicQos(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false
        );

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (_, eventArgs) =>
        {
            try
            {
                var body = eventArgs.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                var message = JsonSerializer.Deserialize<SensorReadingCreatedMessage>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );

                if (message is null)
                {
                    _logger.LogWarning("Received an invalid sensor reading message.");
                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                    return;
                }

                _logger.LogInformation(
                    "Sensor reading received. SensorReadingId: {SensorReadingId}, SensorId: {SensorId}, Temperature: {Temperature}, SoilMoisture: {SoilMoisture}",
                    message.SensorReadingId,
                    message.SensorId,
                    message.Temperature,
                    message.SoilMoisture
                );

                _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing sensor reading message.");

                _channel.BasicNack(
                    eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: true
                );
            }
        };

        _channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer
        );

        _logger.LogInformation(
            "Aruje Worker is listening to RabbitMQ queue: {QueueName}",
            queueName
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();

        base.Dispose();
    }
}