using System.Net.Http.Json;
using ProTodo.Api.DTOs;

namespace ProTodo.Api.Tests;

public static class ApiTestHelpers
{
    public static async Task<ProjectResponse> CreateProjectAsync(this HttpClient client, string name = "Test project")
    {
        var response = await client.PostAsJsonAsync("/api/projects", new { name });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProjectResponse>())!;
    }

    public static async Task<TaskResponse> CreateTaskAsync(
        this HttpClient client, int projectId, string title = "Test task", string? description = null)
    {
        var response = await client.PostAsJsonAsync($"/api/projects/{projectId}/tasks", new { title, description });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskResponse>())!;
    }

    public static async Task<TaskResponse> SetStatusAsync(this HttpClient client, TaskResponse task, string status)
    {
        var response = await client.PutAsJsonAsync($"/api/tasks/{task.Id}",
            new { title = task.Title, description = task.Description, status });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskResponse>())!;
    }

    public static async Task<string> ReadMessageAsync(this HttpResponseMessage response)
    {
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return error!.Message;
    }
}
