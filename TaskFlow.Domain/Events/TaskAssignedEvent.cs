using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

public sealed record TaskAssignedEvent(
    Guid TaskId,
    Guid OrganizationId,
    Guid? PreviousAssigneeId,
    Guid? NewAssigneeId) : IDomainEvent;
