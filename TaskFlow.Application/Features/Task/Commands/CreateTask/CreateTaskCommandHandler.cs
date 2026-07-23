using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.Commands.CreateTask;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateTaskCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId ?? throw new ForbiddenAccessException();
        var userId = _currentUser.UserId ?? throw new ForbiddenAccessException();

        // Tenant-scoped existence check: a project ID from another org must 404, not succeed.
        var projectExists = await _context.Projects
            .AnyAsync(p => p.Id == request.ProjectId && p.OrganizationId == organizationId, cancellationToken);

        if (!projectExists)
        {
            throw new NotFoundException(nameof(Project), request.ProjectId);
        }

        var task = TaskItem.Create(
            organizationId,
            request.ProjectId,
            request.Title,
            request.Description,
            request.Priority,
            request.AssigneeId,
            request.DueDate,
            userId);

        _context.Tasks.Add(task);

        // TaskItem.Create() already raised a TaskCreatedEvent internally. In Step 3, the
        // SaveChangesInterceptor reads task.DomainEvents here, writes them to
        // OutboxMessages in this same SaveChanges call, then clears them - so the task
        // row and its outbox event are committed atomically or not at all.
        await _context.SaveChangesAsync(cancellationToken);

        return task.Id;
    }
}
