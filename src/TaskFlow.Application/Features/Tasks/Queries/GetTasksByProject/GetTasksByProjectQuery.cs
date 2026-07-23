using MediatR;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.Application.Features.Tasks.Queries.GetTasksByProject;

public record GetTasksByProjectQuery(
    Guid ProjectId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PaginatedList<TaskDto>>;
