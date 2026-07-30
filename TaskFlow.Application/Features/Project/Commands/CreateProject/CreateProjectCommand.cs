using MediatR;

namespace TaskFlow.Application.Features.Projects.Commands.CreateProject;

// Same pattern as CreateTaskCommand - no OrganizationId or CreatedByUserId here, both come
// from ICurrentUserService inside the handler.
public record CreateProjectCommand(string Name, string? Description) : IRequest<Guid>;
