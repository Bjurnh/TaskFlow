using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Features.Projects.Queries.GetProjects;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, List<ProjectDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetProjectsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId;

        var query = _context.Projects
            .Where(p => p.OrganizationId == organizationId);

        if (!request.IncludeArchived)
        {
            query = query.Where(p => p.ArchivedAt == null);
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectDto(p.Id, p.Name, p.Description, p.IsArchived, p.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
