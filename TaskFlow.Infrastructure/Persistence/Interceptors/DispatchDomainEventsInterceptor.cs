using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TaskFlow.Domain.Common;
using TaskFlow.Infrastructure.Persistence.Outbox;

namespace TaskFlow.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Runs BEFORE the transaction commits (SavingChangesAsync, not SavedChangesAsync) so every
/// OutboxMessage row lands in the SAME database transaction as the aggregate's state change.
/// Either both persist, or neither does. If this instead ran in SavedChangesAsync, a crash
/// between the state change committing and the event being recorded would silently lose
/// the event - the exact failure mode the Outbox pattern exists to prevent.
/// </summary>
public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var aggregatesWithEvents = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count > 0)
            .ToList();

        var outboxMessages = aggregatesWithEvents
            .SelectMany(aggregate => aggregate.DomainEvents)
            .Select(domainEvent => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventType = domainEvent.GetType().Name,
                // Serialize against the CONCRETE runtime type, not IDomainEvent - serializing
                // against the interface would produce "{}" since System.Text.Json only sees
                // the interface's (empty) member list at that point.
                Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                OccurredAt = DateTime.UtcNow,
                RetryCount = 0
            })
            .ToList();

        if (outboxMessages.Count > 0)
        {
            context.Set<OutboxMessage>().AddRange(outboxMessages);
        }

        // Clear AFTER building the outbox messages, BEFORE SaveChanges actually runs -
        // otherwise a retried SaveChanges call (e.g. after a transient failure) would
        // re-add duplicate outbox rows for the same events.
        foreach (var aggregate in aggregatesWithEvents)
        {
            aggregate.ClearDomainEvents();
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
