using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

public sealed record TaskCompletedEvent(
    Guid TaskId,
    Guid OrganizationId,
    Guid ProjectId,
    DateTime CompletedAt) : IDomainEvent;
