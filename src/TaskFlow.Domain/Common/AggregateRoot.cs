namespace TaskFlow.Domain.Common;

/// <summary>
/// An Entity that is the root of a consistency boundary (an "aggregate" in DDD terms).
/// Aggregate roots are the only entities the Application layer is allowed to load and
/// persist directly - everything else inside the aggregate is reached through it.
/// It also collects domain events raised by business logic, so the SaveChanges
/// interceptor can turn them into Outbox messages before they're cleared.
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
