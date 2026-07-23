using MediatR;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Features.Tasks.Commands.CreateTask;

// Deliberately NO OrganizationId or CreatedByUserId on this command - those come from
// ICurrentUserService inside the handler, sourced from the JWT. Never trust the client
// to tell you which tenant it belongs to; that's how cross-tenant data leaks happen.
public record CreateTaskCommand(
    Guid ProjectId,
    string Title,
    string? Description,
    TaskPriority Priority,
    Guid? AssigneeId,
    DateTime? DueDate) : IRequest<Guid>;
