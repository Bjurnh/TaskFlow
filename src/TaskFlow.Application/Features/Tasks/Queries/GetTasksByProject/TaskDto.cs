using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Tasks.Queries.GetTasksByProject;

public record TaskDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    TaskStatus Status,
    TaskPriority Priority,
    Guid? AssigneeId,
    DateTime? DueDate,
    DateTime? CompletedAt,
    DateTime CreatedAt);
