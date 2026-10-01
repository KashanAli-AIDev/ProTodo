using Microsoft.EntityFrameworkCore;
using ProTodo.Api.Data;
using ProTodo.Api.DTOs;
using ProTodo.Api.Entities;

namespace ProTodo.Api.Services;

// Methods return null/false when the project or task does not exist; the controller maps that to 404.
public class TaskService(AppDbContext db)
{
    public async Task<TaskResponse?> CreateAsync(int projectId, CreateTaskRequest request)
    {
        if (!await db.Projects.AnyAsync(p => p.Id == projectId))
            return null;

        var now = DateTime.UtcNow;
        var task = new TaskItem
        {
            ProjectId = projectId,
            Title = request.Title!.Trim(),
            Description = NormalizeDescription(request.Description),
            Status = TaskItemStatus.Pending,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        return ToResponse(task);
    }

    public async Task<List<TaskResponse>?> ListAsync(int projectId, TaskItemStatus? status)
    {
        if (!await db.Projects.AnyAsync(p => p.Id == projectId))
            return null;

        var query = db.Tasks.AsNoTracking().Where(t => t.ProjectId == projectId);
        if (status is not null)
            query = query.Where(t => t.Status == status);

        var tasks = await query.OrderBy(t => t.Id).ToListAsync();
        return tasks.Select(ToResponse).ToList();
    }

    public async Task<TaskResponse?> UpdateAsync(int id, UpdateTaskRequest request)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task is null)
            return null;

        // ProjectId is deliberately not part of the request: a task cannot be moved between projects.
        task.Title = request.Title!.Trim();
        task.Description = NormalizeDescription(request.Description);
        TaskItemStatusParser.TryParse(request.Status, out var status); // already validated on the DTO
        task.Status = status;
        task.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return ToResponse(task);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task is null)
            return false;

        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return true;
    }

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    private static TaskResponse ToResponse(TaskItem t) =>
        new(t.Id, t.ProjectId, t.Title, t.Description, t.Status.ToString(), t.CreatedAtUtc, t.UpdatedAtUtc);
}
