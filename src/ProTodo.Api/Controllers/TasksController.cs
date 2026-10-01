using Microsoft.AspNetCore.Mvc;
using ProTodo.Api.DTOs;
using ProTodo.Api.Entities;
using ProTodo.Api.Services;

namespace ProTodo.Api.Controllers;

[ApiController]
[Route("api")]
public class TasksController(TaskService tasks) : ControllerBase
{
    private static readonly ErrorResponse ProjectNotFound = new("Project was not found.");
    private static readonly ErrorResponse TaskNotFound = new("Task was not found.");

    [HttpPost("projects/{projectId:int}/tasks")]
    public async Task<ActionResult<TaskResponse>> Create(int projectId, CreateTaskRequest request)
    {
        var created = await tasks.CreateAsync(projectId, request);
        return created is null
            ? NotFound(ProjectNotFound)
            : StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpGet("projects/{projectId:int}/tasks")]
    public async Task<ActionResult<List<TaskResponse>>> List(int projectId, [FromQuery] string? status)
    {
        TaskItemStatus? filter = null;
        // MVC binds "?status=" to null, so check the raw query to reject an empty status instead of ignoring it.
        if (Request.Query.ContainsKey("status"))
        {
            if (!TaskItemStatusParser.TryParse(status, out var parsed))
                return BadRequest(new ErrorResponse(TaskItemStatusParser.ValidValuesMessage));
            filter = parsed;
        }

        var result = await tasks.ListAsync(projectId, filter);
        return result is null ? NotFound(ProjectNotFound) : result;
    }

    [HttpPut("tasks/{id:int}")]
    public async Task<ActionResult<TaskResponse>> Update(int id, UpdateTaskRequest request)
    {
        var updated = await tasks.UpdateAsync(id, request);
        return updated is null ? NotFound(TaskNotFound) : updated;
    }

    [HttpDelete("tasks/{id:int}")]
    public async Task<IActionResult> Delete(int id) =>
        await tasks.DeleteAsync(id) ? NoContent() : NotFound(TaskNotFound);
}
