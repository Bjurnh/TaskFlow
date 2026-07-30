using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Features.Projects.Commands.CreateProject;
using TaskFlow.Application.Features.Projects.Queries.GetProjects;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly ISender _sender;

    public ProjectsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, id);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProjectDto>>> GetAll(
        [FromQuery] bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetProjectsQuery(includeArchived), cancellationToken);
        return Ok(result);
    }
}
