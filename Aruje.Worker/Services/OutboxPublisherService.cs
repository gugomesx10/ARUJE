using System.Text.Json;
using Aruje.Application.Interfaces.Messaging;
using Aruje.Application.Interfaces.Persistence;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Messaging;

namespace Aruje.Worker.Services;

public class OutboxPublisherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPublisherService> _logger;

    public OutboxPublisherService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxPublisherService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Publisher Service started.");

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await PublishPendingMessagesAsync(stoppingToken);
        }
    }

    private async Task PublishPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
        var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var pendingMessages = await outboxRepository.GetPendingAsync(10);

        if (!pendingMessages.Any())
            return;

        foreach (var outboxMessage in pendingMessages)
        {
            try
            {
                if (outboxMessage.EventType != nameof(SensorReadingCreatedMessage))
                {
                    outboxMessage.MarkAsFailed(
                        $"Unsupported event type: {outboxMessage.EventType}"
                    );

                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    continue;
                }

                var message = JsonSerializer.Deserialize<SensorReadingCreatedMessage>(
                    outboxMessage.Payload,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );

                if (message is null)
                {
                    outboxMessage.MarkAsFailed("Invalid outbox payload.");

                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    continue;
                }

                await messagePublisher.PublishSensorReadingCreatedAsync(
                    message,
                    cancellationToken
                );

                outboxMessage.MarkAsProcessed();

                await unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Outbox message published. OutboxMessageId: {OutboxMessageId}",
                    outboxMessage.Id
                );
            }
            catch (Exception ex)
            {
                outboxMessage.MarkAsFailed(ex.Message);

                await unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogError(
                    ex,
                    "Error publishing outbox message. OutboxMessageId: {OutboxMessageId}",
                    outboxMessage.Id
                );
            }
        }
    }
}