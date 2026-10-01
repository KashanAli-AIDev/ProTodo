using Microsoft.EntityFrameworkCore;
using ProTodo.Api.Configuration;
using ProTodo.Api.Data;
using ProTodo.Api.DTOs;
using ProTodo.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = ValidationErrorResponse.Create);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// The API key comes from configuration (user-secrets or the ApiKey environment variable);
// the app refuses to start without one rather than falling back to a default.
builder.Services.AddOptions<ApiKeyOptions>()
    .Configure<IConfiguration>((options, config) => options.ApiKey = config["ApiKey"] ?? string.Empty)
    .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey), "The ApiKey setting must be configured.")
    .ValidateOnStart();

builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<TaskService>();

var app = builder.Build();

// The exception handler middleware logs the exception server-side; clients only get a generic message.
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new ErrorResponse("An unexpected error occurred."));
}));

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.Run();

// Exposed so integration tests can use WebApplicationFactory<Program>.
public partial class Program { }
