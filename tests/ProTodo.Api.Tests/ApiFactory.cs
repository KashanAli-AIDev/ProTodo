using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProTodo.Api.Configuration;
using ProTodo.Api.Data;

namespace ProTodo.Api.Tests;

// Boots the real API against its own uniquely named SQL Server LocalDB database.
// The database is created from the EF Core migrations (so they are exercised too) and dropped afterwards.
// Each test class that uses this fixture gets a separate database, so classes never share data.
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string ApiKey = "test-api-key-not-a-secret";

    private const string LocalDbServer = @"Server=(localdb)\MSSQLLocalDB;";
    private readonly string _databaseName = $"ProTodo_Test_{Guid.NewGuid():N}";

    // Subclasses that point at an unreachable database turn this off.
    protected virtual bool ManagesDatabase => true;

    protected virtual string ConnectionString =>
        $"{LocalDbServer}Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" keeps the developer's user-secrets and appsettings.Development.json out of the tests.
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = ConnectionString,
            ["ApiKey"] = ApiKey
        }));
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(ApiKeyMiddleware.HeaderName, ApiKey);
        return client;
    }

    public async Task InitializeAsync()
    {
        if (!ManagesDatabase)
            return;

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        if (ManagesDatabase)
        {
            using var scope = Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureDeletedAsync();
        }

        await base.DisposeAsync();
    }
}
