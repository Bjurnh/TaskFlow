using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Events;

public sealed record TaskStatusChangedEvent(
    Guid TaskId,
    Guid OrganizationId,
    TaskStatus OldStatus,
    TaskStatus NewStatus) : IDomainEvent;
