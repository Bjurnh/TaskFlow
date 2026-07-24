using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common.Models;
using TaskFlow.Application.Features.Tasks.Commands.CreateTask;
using TaskFlow.Application.Features.Tasks.Commands.UpdateTaskStatus;
using TaskFlow.Application.Features.Tasks.Queries.GetTasksByProject;
// NOTE: if your Domain project renamed this enum to WorkItemStatus (per the earlier
// naming-collision discussion), change this alias target accordingly. Aliasing here means
// every other line in this file can just say "TaskStatusDto" - no risk of it silently
// binding to System.Threading.Tasks.TaskStatus the way a bare reference could.
using TaskStatusDto = TaskFlow.Domain.Enums.WorkItemStatus;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ISender _sender;

    public TasksController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByProject), new { projectId = command.ProjectId }, id);
    }

    [HttpGet("by-project/{projectId:guid}")]
    [ProducesResponseType(typeof(PaginatedList<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<TaskDto>>> GetByProject(
        Guid projectId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetTasksByProjectQuery(projectId, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new UpdateTaskStatusCommand(id, request.NewStatus), cancellationToken);
        return NoContent();
    }
}

public record UpdateTaskStatusRequest(TaskStatusDto NewStatus);
