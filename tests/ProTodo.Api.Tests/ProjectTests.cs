using System.Net;
using System.Net.Http.Json;
using System.Text;
using ProTodo.Api.DTOs;

namespace ProTodo.Api.Tests;

public class ProjectTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateAuthenticatedClient();

    [Fact]
    public async Task Create_valid_project_returns_201_with_trimmed_name()
    {
        var response = await _client.PostAsJsonAsync("/api/projects", new { name = "  Website Development  " });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var project = await response.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.True(project!.Id > 0);
        Assert.Equal("Website Development", project.Name);
        Assert.Equal(DateTimeKind.Utc, project.CreatedAtUtc.Kind);
    }

    [Fact]
    public async Task Create_project_with_name_at_max_length_succeeds()
    {
        var response = await _client.PostAsJsonAsync("/api/projects", new { name = new string('a', 100) });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"name\":null}")]
    [InlineData("{\"name\":\"\"}")]
    [InlineData("{\"name\":\"   \"}")]
    public async Task Create_project_without_a_usable_name_returns_400(string json)
    {
        var response = await _client.PostAsync("/api/projects", Json(json));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Project name is required.", await response.ReadMessageAsync());
    }

    [Fact]
    public async Task Create_project_with_too_long_name_returns_400()
    {
        var response = await _client.PostAsJsonAsync("/api/projects", new { name = new string('a', 101) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Project name must be at most 100 characters.", await response.ReadMessageAsync());
    }

    [Theory]
    [InlineData("{not json")]
    [InlineData("{\"name\":123}")]
    public async Task Create_project_with_malformed_body_returns_400_without_parser_details(string json)
    {
        var response = await _client.PostAsync("/api/projects", Json(json));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var message = await response.ReadMessageAsync();
        Assert.Equal("The request body is not valid JSON or has fields of the wrong type.", message);
    }

    [Fact]
    public async Task List_projects_returns_200_with_created_projects()
    {
        var first = await _client.CreateProjectAsync("List test A");
        var second = await _client.CreateProjectAsync("List test B");

        var response = await _client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var projects = await response.Content.ReadFromJsonAsync<List<ProjectResponse>>();
        Assert.Contains(projects!, p => p.Id == first.Id && p.Name == "List test A");
        Assert.Contains(projects!, p => p.Id == second.Id && p.Name == "List test B");
        // Values read back from SQL Server must still be flagged as UTC (serialized with a trailing "Z").
        Assert.All(projects!, p => Assert.Equal(DateTimeKind.Utc, p.CreatedAtUtc.Kind));
    }

    private static StringContent Json(string json) => new(json, Encoding.UTF8, "application/json");
}
