using System.Net;
using System.Net.Http.Json;

namespace ProTodo.Api.Tests;

public class AuthenticationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    public static TheoryData<string, string> AllEndpoints => new()
    {
        { "POST", "/api/projects" },
        { "GET", "/api/projects" },
        { "POST", "/api/projects/1/tasks" },
        { "GET", "/api/projects/1/tasks" },
        { "PUT", "/api/tasks/1" },
        { "DELETE", "/api/tasks/1" }
    };

    [Theory]
    [MemberData(nameof(AllEndpoints))]
    public async Task Missing_api_key_returns_401_on_every_endpoint(string method, string url)
    {
        using var client = factory.CreateClient();
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method is "POST" or "PUT")
            request.Content = JsonContent.Create(new { name = "x", title = "x", status = "Pending" });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("A valid X-API-Key header is required.", await response.ReadMessageAsync());
    }

    [Theory]
    [InlineData("wrong-key")]
    [InlineData("")]
    [InlineData("test-api-key-not-a-secret-plus-extra")]
    [InlineData("TEST-API-KEY-NOT-A-SECRET")]
    public async Task Invalid_api_key_returns_401(string key)
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-Key", key);

        var response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Valid_api_key_succeeds()
    {
        using var client = factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_route_without_api_key_returns_401_not_404()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/does-not-exist");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
