namespace TaskFlow.Application.Features.Projects.Queries.GetProjects;

public record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsArchived,
    DateTime CreatedAt);
