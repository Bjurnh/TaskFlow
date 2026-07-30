using MediatR;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Features.Projects.Commands.CreateProject;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateProjectCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId ?? throw new ForbiddenAccessException();
        var userId = _currentUser.UserId ?? throw new ForbiddenAccessException();

        var project = Project.Create(organizationId, request.Name, request.Description, userId);

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
