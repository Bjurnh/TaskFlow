using MediatR;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.Application.Features.Tasks.Queries.GetTasksByProject;

public class GetTasksByProjectQueryHandler : IRequestHandler<GetTasksByProjectQuery, PaginatedList<TaskDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTasksByProjectQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<TaskDto>> Handle(GetTasksByProjectQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId;

        // Projected directly to TaskDto in the LINQ expression - EF Core translates this
        // to a SELECT of only the needed columns, never materializing full TaskItem entities
        // for a read-only query. AsNoTracking-equivalent by construction (no tracked entity
        // graph is created when you project to a non-entity type).
        var query = _context.Tasks
            .Where(t => t.ProjectId == request.ProjectId && t.OrganizationId == organizationId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TaskDto(
                t.Id,
                t.ProjectId,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.AssigneeId,
                t.DueDate,
                t.CompletedAt,
                t.CreatedAt));

        return await PaginatedList<TaskDto>.CreateAsync(query, request.PageNumber, request.PageSize, cancellationToken);
    }
}
