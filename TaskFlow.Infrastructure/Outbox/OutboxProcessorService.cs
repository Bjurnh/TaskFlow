using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskFlow.Domain.Common;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Persistence.Outbox;

namespace TaskFlow.Infrastructure.Outbox;

/// <summary>
/// Polls OutboxMessages for unprocessed rows and dispatches each one through MediatR's
/// IPublisher, which routes it to whatever INotificationHandler{T} classes are registered
/// for that concrete event type (see EventHandlers/). This is the piece that actually
/// closes the loop: DispatchDomainEventsInterceptor writes events durably at command time,
/// this service is what turns "durably recorded" into "actually acted upon."
///
/// Runs as a singleton hosted service, but creates a new DI scope per poll iteration -
/// DbContext and IPublisher are scoped, so they can't be injected directly into a
/// BackgroundService's constructor (which lives for the app's whole lifetime).
/// </summary>
public class OutboxProcessorService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;
    private const int MaxRetryCount = 5;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessorService> _logger;

    public OutboxProcessorService(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        // Run once immediately on startup, then on every timer tick - otherwise the first
        // batch of events would sit unprocessed for a full PollInterval after a deploy/restart.
        do
        {
            try
            {
                await ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // A failure processing ONE batch should never kill the whole background
                // service - log it and let the next poll try again.
                _logger.LogError(ex, "Unhandled error while processing outbox messages");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskFlowDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < MaxRetryCount)
            .OrderBy(m => m.OccurredAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            await ProcessMessageAsync(message, publisher, cancellationToken);
        }

        // One SaveChanges for the whole batch - each message's ProcessedAt/Error/RetryCount
        // was already mutated in-memory by ProcessMessageAsync below.
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessMessageAsync(OutboxMessage message, IPublisher publisher, CancellationToken cancellationToken)
    {
        try
        {
            var eventType = DomainEventTypeRegistry.Resolve(message.EventType);

            if (eventType is null)
            {
                // A stored EventType string that doesn't match any known IDomainEvent type
                // is a data/deployment problem, not something worth retrying - fail it
                // permanently rather than looping on it forever.
                message.Error = $"Unknown event type '{message.EventType}' - no matching IDomainEvent implementation found.";
                message.RetryCount = MaxRetryCount;
                _logger.LogError("Outbox message {MessageId} references unknown event type {EventType}",
                    message.Id, message.EventType);
                return;
            }

            var domainEvent = (IDomainEvent?)JsonSerializer.Deserialize(message.Payload, eventType);

            if (domainEvent is null)
            {
                message.Error = "Deserialization returned null.";
                message.RetryCount++;
                return;
            }

            await publisher.Publish(domainEvent, cancellationToken);

            message.ProcessedAt = DateTime.UtcNow;
            message.Error = null;
        }
        catch (Exception ex)
        {
            message.RetryCount++;
            message.Error = ex.Message;
            _logger.LogWarning(ex, "Failed processing outbox message {MessageId} (attempt {RetryCount}/{MaxRetryCount})",
                message.Id, message.RetryCount, MaxRetryCount);
        }
    }
}
