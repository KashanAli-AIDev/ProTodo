using Microsoft.AspNetCore.Mvc;
using ProTodo.Api.DTOs;
using ProTodo.Api.Services;

namespace ProTodo.Api.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController(ProjectService projects) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create(CreateProjectRequest request)
    {
        var created = await projects.CreateAsync(request);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjectResponse>>> List() =>
        await projects.ListAsync();
}
