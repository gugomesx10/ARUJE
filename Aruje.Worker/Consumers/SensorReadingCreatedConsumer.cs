using System.Text;
using System.Text.Json;
using Aruje.Application.Interfaces.Persistence;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Interfaces.Services;
using Aruje.Application.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Aruje.Worker.Consumers;

public class SensorReadingCreatedConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SensorReadingCreatedConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private IConnection? _connection;
    private IModel? _channel;

    public SensorReadingCreatedConsumer(
        IConfiguration configuration,
        ILogger<SensorReadingCreatedConsumer> logger,
        IServiceScopeFactory scopeFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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
            Password = password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: exchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );

        _channel.ExchangeDeclare(
            exchange: deadLetterExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );

        _channel.QueueDeclare(
            queue: deadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        _channel.QueueBind(
            queue: deadLetterQueueName,
            exchange: deadLetterExchangeName,
            routingKey: deadLetterRoutingKey
        );

        var queueArguments = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = deadLetterExchangeName,
            ["x-dead-letter-routing-key"] = deadLetterRoutingKey
        };

        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments
        );

        _channel.QueueBind(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey
        );

        _channel.BasicQos(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false
        );

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (_, eventArgs) =>
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
                    _logger.LogWarning("Received an invalid sensor reading message. Sending to DLQ.");

                    _channel.BasicNack(
                        eventArgs.DeliveryTag,
                        multiple: false,
                        requeue: false
                    );

                    return;
                }

                await ProcessMessageAsync(message, stoppingToken);

                _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing sensor reading message. Sending to DLQ.");

                _channel.BasicNack(
                    eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: false
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

        _logger.LogInformation(
            "Dead Letter Queue configured: {DeadLetterQueueName}",
            deadLetterQueueName
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessMessageAsync(
        SensorReadingCreatedMessage message,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var sensorReadingRepository = scope.ServiceProvider.GetRequiredService<ISensorReadingRepository>();
        var alertRepository = scope.ServiceProvider.GetRequiredService<IAlertRepository>();
        var aiAnalysisRepository = scope.ServiceProvider.GetRequiredService<IAiAnalysisRepository>();
        var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
        var aiAnalysisService = scope.ServiceProvider.GetRequiredService<IAiAnalysisService>();
        var pushNotificationService = scope.ServiceProvider.GetRequiredService<IPushNotificationService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var reading = await sensorReadingRepository.GetByIdAsync(message.SensorReadingId);

        if (reading is null || !reading.IsActive)
        {
            _logger.LogWarning(
                "Sensor reading not found or inactive. SensorReadingId: {SensorReadingId}",
                message.SensorReadingId
            );

            return;
        }

        var existingAlerts = await alertRepository.GetBySensorReadingIdAsync(reading.Id);

        if (existingAlerts.Any())
        {
            _logger.LogInformation(
                "Sensor reading already processed. SensorReadingId: {SensorReadingId}",
                reading.Id
            );

            return;
        }

        var alert = await alertService.GenerateAlertFromReadingAsync(
            reading,
            cancellationToken
        );

        if (alert is null)
        {
            _logger.LogInformation(
                "Sensor reading processed without alert. SensorReadingId: {SensorReadingId}",
                reading.Id
            );

            return;
        }

        await alertRepository.AddAsync(alert);

        var aiAnalysis = await aiAnalysisService.GenerateAnalysisAsync(
            alert,
            reading,
            cancellationToken
        );

        await aiAnalysisRepository.AddAsync(aiAnalysis);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (alert.Severity == Aruje.Domain.Enums.AlertSeverity.High)
        {
            await pushNotificationService.SendAlertNotificationAsync(
                alert,
                cancellationToken
            );
        }

        _logger.LogInformation(
            "Alert, AI analysis and push notification flow processed by Worker. SensorReadingId: {SensorReadingId}, AlertId: {AlertId}",
            reading.Id,
            alert.Id
        );
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();

        base.Dispose();
    }
}