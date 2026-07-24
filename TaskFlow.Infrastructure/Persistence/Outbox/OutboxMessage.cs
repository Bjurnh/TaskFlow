namespace TaskFlow.Infrastructure.Persistence.Outbox;

/// <summary>
/// Infrastructure-only concept - Application never queries this directly, it just raises
/// IDomainEvents on aggregates. This is the durable record of "an event happened and still
/// needs delivering," written in the SAME transaction as the aggregate's state change by
/// DispatchDomainEventsInterceptor. A background service (Step 5) polls rows where
/// ProcessedAt IS NULL and dispatches them.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public DateTime OccurredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; }
}
