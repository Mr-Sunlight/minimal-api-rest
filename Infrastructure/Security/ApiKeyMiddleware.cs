namespace API_REST_WITH_MINIMAL_API.Infrastructure.Security;

public class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private const string ApiKeyHeaderName = "X-API-KEY";

    public async Task InvokeAsync(HttpContext context)
    {
        // Allow Swagger UI and OpenAPI docs without API key
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var configuredApiKey = configuration["ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredApiKey))
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "API Key is not configured on the server." });
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedApiKey) ||
            !string.Equals(providedApiKey.ToString(), configuredApiKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized. Missing or invalid X-API-KEY header." });
            return;
        }

        await next(context);
    }
}
