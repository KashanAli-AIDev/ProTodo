using System.Net;

namespace ProTodo.Api.Tests;

// The database is deliberately unreachable (nonexistent database, 1 second timeout) to force an unexpected failure.
public class UnreachableDatabaseFactory : ApiFactory
{
    protected override string ConnectionString =>
        @"Server=(localdb)\MSSQLLocalDB;Database=ProTodo_DoesNotExist;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=1";

    protected override bool ManagesDatabase => false;
}

public class ErrorHandlingTests(UnreachableDatabaseFactory factory) : IClassFixture<UnreachableDatabaseFactory>
{
    [Fact]
    public async Task Unexpected_failure_returns_generic_500_without_internal_details()
    {
        using var client = factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/projects");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("""{"message":"An unexpected error occurred."}""", body);
        Assert.DoesNotContain("localdb", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SqlException", body);
        Assert.DoesNotContain("   at ", body); // stack trace frames
    }
}
