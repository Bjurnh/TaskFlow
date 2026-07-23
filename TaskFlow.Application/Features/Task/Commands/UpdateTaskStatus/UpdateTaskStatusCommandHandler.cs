using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Tasks.Commands.UpdateTaskStatus;

public class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateTaskStatusCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId ?? throw new ForbiddenAccessException();

        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskId && t.OrganizationId == organizationId, cancellationToken);

        if (task is null)
        {
            // nameof(TaskItem), not nameof(Task) - the latter would silently resolve to
            // System.Threading.Tasks.Task since it's implicitly in scope on async methods.
            throw new NotFoundException(nameof(TaskItem), request.TaskId);
        }

        // All the branching (Done vs not, raising TaskCompletedEvent vs just
        // TaskStatusChangedEvent) lives inside the aggregate, not here. The handler's job
        // is orchestration only - load, call behavior, save. No business rules leak into
        // the Application layer.
        task.ChangeStatus(request.NewStatus);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
