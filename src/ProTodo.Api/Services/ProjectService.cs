using Microsoft.EntityFrameworkCore;
using ProTodo.Api.Data;
using ProTodo.Api.DTOs;
using ProTodo.Api.Entities;

namespace ProTodo.Api.Services;

public class ProjectService(AppDbContext db)
{
    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request)
    {
        var project = new Project
        {
            Name = request.Name!.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Projects.Add(project);
        await db.SaveChangesAsync();

        return ToResponse(project);
    }

    public async Task<List<ProjectResponse>> ListAsync() =>
        await db.Projects
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Select(p => new ProjectResponse(p.Id, p.Name, p.CreatedAtUtc))
            .ToListAsync();

    private static ProjectResponse ToResponse(Project p) => new(p.Id, p.Name, p.CreatedAtUtc);
}
