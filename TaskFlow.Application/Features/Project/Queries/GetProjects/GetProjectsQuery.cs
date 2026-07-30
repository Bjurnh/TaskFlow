using MediatR;

namespace TaskFlow.Application.Features.Projects.Queries.GetProjects;

public record GetProjectsQuery(bool IncludeArchived = false) : IRequest<List<ProjectDto>>;
