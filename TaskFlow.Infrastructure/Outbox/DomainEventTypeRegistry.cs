using TaskFlow.Domain.Common;

namespace TaskFlow.Infrastructure.Outbox;

/// <summary>
/// OutboxMessage.EventType stores just the short type name (e.g. "TaskCompletedEvent"),
/// not an assembly-qualified name - so the processor needs a way to turn that string back
/// into a real CLR Type before it can JsonSerializer.Deserialize the payload.
///
/// Built once via reflection over the Domain assembly rather than a hand-maintained
/// switch statement - every type implementing IDomainEvent is picked up automatically,
/// so adding a new domain event later requires zero changes here.
/// </summary>
public static class DomainEventTypeRegistry
{
    private static readonly Lazy<IReadOnlyDictionary<string, Type>> TypesByName = new(() =>
        typeof(IDomainEvent).Assembly
            .GetTypes()
            .Where(t => typeof(IDomainEvent).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
            .ToDictionary(t => t.Name, t => t));

    public static Type? Resolve(string eventTypeName) =>
        TypesByName.Value.TryGetValue(eventTypeName, out var type) ? type : null;
}
