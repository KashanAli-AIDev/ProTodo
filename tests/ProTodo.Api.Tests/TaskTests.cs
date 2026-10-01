using System.Net;
using System.Net.Http.Json;
using ProTodo.Api.DTOs;

namespace ProTodo.Api.Tests;

public class TaskTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string ValidStatusMessage = "Status must be one of: Pending, InProgress, Completed.";

    private readonly HttpClient _client = factory.CreateAuthenticatedClient();

    // ---- Create ----

    [Fact]
    public async Task Create_task_returns_201_and_defaults_to_Pending()
    {
        var project = await _client.CreateProjectAsync();

        var response = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/tasks",
            new { title = "  Implement login API  ", description = "Create the authentication endpoint" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.True(task!.Id > 0);
        Assert.Equal(project.Id, task.ProjectId);
        Assert.Equal("Implement login API", task.Title);
        Assert.Equal("Create the authentication endpoint", task.Description);
        Assert.Equal("Pending", task.Status);
        Assert.Equal(task.CreatedAtUtc, task.UpdatedAtUtc);
    }

    [Fact]
    public async Task Create_task_without_description_succeeds()
    {
        var project = await _client.CreateProjectAsync();

        var task = await _client.CreateTaskAsync(project.Id, "No description");

        Assert.Null(task.Description);
    }

    [Fact]
    public async Task Create_task_for_nonexistent_project_returns_404()
    {
        var response = await _client.PostAsJsonAsync("/api/projects/999999/tasks", new { title = "Orphan" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Project was not found.", await response.ReadMessageAsync());
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"title\":\"\"}")]
    [InlineData("{\"title\":\"   \"}")]
    public async Task Create_task_without_a_usable_title_returns_400(string json)
    {
        var project = await _client.CreateProjectAsync();

        var response = await _client.PostAsync($"/api/projects/{project.Id}/tasks",
            new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Task title is required.", await response.ReadMessageAsync());
    }

    [Fact]
    public async Task Create_task_with_too_long_title_or_description_returns_400()
    {
        var project = await _client.CreateProjectAsync();

        var longTitle = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/tasks",
            new { title = new string('t', 201) });
        var longDescription = await _client.PostAsJsonAsync($"/api/projects/{project.Id}/tasks",
            new { title = "ok", description = new string('d', 2001) });

        Assert.Equal(HttpStatusCode.BadRequest, longTitle.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, longDescription.StatusCode);
    }

    [Fact]
    public async Task Validation_runs_before_project_lookup()
    {
        var response = await _client.PostAsJsonAsync("/api/projects/999999/tasks", new { title = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ---- List and filter ----

    [Fact]
    public async Task List_returns_only_the_tasks_of_the_requested_project()
    {
        var projectA = await _client.CreateProjectAsync("A");
        var projectB = await _client.CreateProjectAsync("B");
        var taskA = await _client.CreateTaskAsync(projectA.Id, "Task in A");
        await _client.CreateTaskAsync(projectB.Id, "Task in B");

        var response = await _client.GetAsync($"/api/projects/{projectA.Id}/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponse>>();
        var only = Assert.Single(tasks!);
        Assert.Equal(taskA.Id, only.Id);
    }

    [Fact]
    public async Task List_for_project_without_tasks_returns_empty_array()
    {
        var project = await _client.CreateProjectAsync();

        var response = await _client.GetAsync($"/api/projects/{project.Id}/tasks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await response.Content.ReadFromJsonAsync<List<TaskResponse>>())!);
    }

    [Fact]
    public async Task List_for_nonexistent_project_returns_404()
    {
        var response = await _client.GetAsync("/api/projects/999999/tasks");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Project was not found.", await response.ReadMessageAsync());
    }

    [Theory]
    [InlineData("Pending", new[] { "pending-task" })]
    [InlineData("InProgress", new[] { "in-progress-task" })]
    [InlineData("Completed", new[] { "completed-task-1", "completed-task-2" })]
    [InlineData("completed", new[] { "completed-task-1", "completed-task-2" })]
    public async Task List_filters_by_status(string filter, string[] expectedTitles)
    {
        var project = await _client.CreateProjectAsync();
        await _client.CreateTaskAsync(project.Id, "pending-task");
        var inProgress = await _client.CreateTaskAsync(project.Id, "in-progress-task");
        var completed1 = await _client.CreateTaskAsync(project.Id, "completed-task-1");
        var completed2 = await _client.CreateTaskAsync(project.Id, "completed-task-2");
        await _client.SetStatusAsync(inProgress, "InProgress");
        await _client.SetStatusAsync(completed1, "Completed");
        await _client.SetStatusAsync(completed2, "Completed");

        var response = await _client.GetAsync($"/api/projects/{project.Id}/tasks?status={filter}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponse>>();
        Assert.Equal(expectedTitles, tasks!.Select(t => t.Title).ToArray());
    }

    [Theory]
    [InlineData("Done")]
    [InlineData("1")]
    [InlineData("")]
    [InlineData("%20")]
    public async Task List_with_invalid_status_returns_400_listing_valid_values(string filter)
    {
        var project = await _client.CreateProjectAsync();
        await _client.CreateTaskAsync(project.Id);

        var response = await _client.GetAsync($"/api/projects/{project.Id}/tasks?status={filter}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ValidStatusMessage, await response.ReadMessageAsync());
    }

    // ---- Update ----

    [Fact]
    public async Task Update_task_returns_200_and_persists_changes()
    {
        var project = await _client.CreateProjectAsync();
        var original = await _client.CreateTaskAsync(project.Id, "Implement login API", "Create endpoint");
        await Task.Delay(20);

        var response = await _client.PutAsJsonAsync($"/api/tasks/{original.Id}", new
        {
            title = "Implement login API v2",
            description = "Authentication endpoint completed",
            status = "Completed"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.Equal(original.Id, updated!.Id);
        Assert.Equal(project.Id, updated.ProjectId);
        Assert.Equal("Implement login API v2", updated.Title);
        Assert.Equal("Authentication endpoint completed", updated.Description);
        Assert.Equal("Completed", updated.Status);
        Assert.Equal(original.CreatedAtUtc, updated.CreatedAtUtc);
        Assert.True(updated.UpdatedAtUtc > original.UpdatedAtUtc);

        var listed = await _client.GetFromJsonAsync<List<TaskResponse>>($"/api/projects/{project.Id}/tasks");
        Assert.Equal("Completed", Assert.Single(listed!).Status);
    }

    [Fact]
    public async Task Update_cannot_move_a_task_to_another_project()
    {
        var home = await _client.CreateProjectAsync("Home");
        var other = await _client.CreateProjectAsync("Other");
        var task = await _client.CreateTaskAsync(home.Id);

        var response = await _client.PutAsJsonAsync($"/api/tasks/{task.Id}",
            new { title = task.Title, status = "Pending", projectId = other.Id });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(home.Id, (await response.Content.ReadFromJsonAsync<TaskResponse>())!.ProjectId);
        Assert.Empty((await _client.GetFromJsonAsync<List<TaskResponse>>($"/api/projects/{other.Id}/tasks"))!);
    }

    [Fact]
    public async Task Update_without_description_clears_it()
    {
        var project = await _client.CreateProjectAsync();
        var task = await _client.CreateTaskAsync(project.Id, "T", "to be cleared");

        var response = await _client.PutAsJsonAsync($"/api/tasks/{task.Id}", new { title = "T", status = "Pending" });

        Assert.Null((await response.Content.ReadFromJsonAsync<TaskResponse>())!.Description);
    }

    [Fact]
    public async Task Update_nonexistent_task_returns_404()
    {
        var response = await _client.PutAsJsonAsync("/api/tasks/999999", new { title = "x", status = "Pending" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Task was not found.", await response.ReadMessageAsync());
    }

    [Theory]
    [InlineData("Done")]
    [InlineData("1")]
    [InlineData("99")]
    public async Task Update_with_invalid_status_returns_400_and_leaves_task_unchanged(string status)
    {
        var project = await _client.CreateProjectAsync();
        var task = await _client.CreateTaskAsync(project.Id);

        var response = await _client.PutAsJsonAsync($"/api/tasks/{task.Id}", new { title = "Changed", status });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(ValidStatusMessage, await response.ReadMessageAsync());
        var current = (await _client.GetFromJsonAsync<List<TaskResponse>>($"/api/projects/{project.Id}/tasks"))!.Single();
        Assert.Equal(task.Title, current.Title);
        Assert.Equal("Pending", current.Status);
    }

    [Fact]
    public async Task Update_with_numeric_json_status_returns_400()
    {
        var project = await _client.CreateProjectAsync();
        var task = await _client.CreateTaskAsync(project.Id);

        var response = await _client.PutAsJsonAsync($"/api/tasks/{task.Id}", new { title = "x", status = 1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_without_status_or_title_returns_400()
    {
        var project = await _client.CreateProjectAsync();
        var task = await _client.CreateTaskAsync(project.Id);

        var noStatus = await _client.PutAsJsonAsync($"/api/tasks/{task.Id}", new { title = "x" });
        var noTitle = await _client.PutAsJsonAsync($"/api/tasks/{task.Id}", new { status = "Pending" });

        Assert.Equal(HttpStatusCode.BadRequest, noStatus.StatusCode);
        Assert.Equal("Task status is required.", await noStatus.ReadMessageAsync());
        Assert.Equal(HttpStatusCode.BadRequest, noTitle.StatusCode);
        Assert.Equal("Task title is required.", await noTitle.ReadMessageAsync());
    }

    // ---- Delete ----

    [Fact]
    public async Task Delete_task_returns_204_and_removes_only_that_task()
    {
        var project = await _client.CreateProjectAsync();
        var doomed = await _client.CreateTaskAsync(project.Id, "Doomed");
        var survivor = await _client.CreateTaskAsync(project.Id, "Survivor");

        var response = await _client.DeleteAsync($"/api/tasks/{doomed.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var remaining = await _client.GetFromJsonAsync<List<TaskResponse>>($"/api/projects/{project.Id}/tasks");
        Assert.Equal(survivor.Id, Assert.Single(remaining!).Id);
    }

    [Fact]
    public async Task Delete_same_task_twice_returns_404_the_second_time()
    {
        var project = await _client.CreateProjectAsync();
        var task = await _client.CreateTaskAsync(project.Id);

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/tasks/{task.Id}")).StatusCode);
        var second = await _client.DeleteAsync($"/api/tasks/{task.Id}");

        Assert.Equal(HttpStatusCode.NotFound, second.StatusCode);
        Assert.Equal("Task was not found.", await second.ReadMessageAsync());
    }

    [Fact]
    public async Task Delete_nonexistent_task_returns_404()
    {
        var response = await _client.DeleteAsync("/api/tasks/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
