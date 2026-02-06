using System.Net;
using System.Text.Json;

namespace PoBabyTouchGc.Api.Middleware;

/// <summary>
/// Global exception handler middleware — catches unhandled exceptions
/// and returns a consistent JSON error response.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in request pipeline");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ArgumentNullException or ArgumentException
                => (HttpStatusCode.BadRequest, "Invalid request parameters"),
            UnauthorizedAccessException
                => (HttpStatusCode.Unauthorized, "Unauthorized access"),
            KeyNotFoundException
                => (HttpStatusCode.NotFound, "Resource not found"),
            InvalidOperationException
                => (HttpStatusCode.Conflict, "Operation conflict"),
            Azure.RequestFailedException azureEx
                => (HttpStatusCode.InternalServerError, $"Azure Table Storage error: {azureEx.Message}"),
            _ => (HttpStatusCode.InternalServerError, "An internal server error occurred")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            success = false,
            message,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }
}
