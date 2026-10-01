using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using ProTodo.Api.DTOs;

namespace ProTodo.Api.Configuration;

public class ApiKeyOptions
{
    public string ApiKey { get; set; } = string.Empty;
}

// Requires a valid X-API-Key header on every request.
public class ApiKeyMiddleware(RequestDelegate next, IOptions<ApiKeyOptions> options)
{
    public const string HeaderName = "X-API-Key";

    // Hash both values so the comparison is fixed-time regardless of the length of the supplied key.
    private readonly byte[] _expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(options.Value.ApiKey));

    public async Task InvokeAsync(HttpContext context)
    {
        var supplied = context.Request.Headers[HeaderName];

        if (supplied.Count != 1 || !IsValid(supplied[0]))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new ErrorResponse("A valid X-API-Key header is required."));
            return;
        }

        await next(context);
    }

    private bool IsValid(string? key) =>
        !string.IsNullOrEmpty(key) &&
        CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(key)), _expectedHash);
}
