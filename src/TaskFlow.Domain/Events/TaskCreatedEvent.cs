using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

public sealed record TaskCreatedEvent(
    Guid TaskId,
    Guid OrganizationId,
    Guid ProjectId,
    string Title,
    DateTime CreatedAt) : IDomainEvent;
