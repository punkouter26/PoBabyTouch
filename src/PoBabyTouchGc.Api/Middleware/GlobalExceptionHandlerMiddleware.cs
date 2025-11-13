using System.Net;
using System.Text.Json;
using PoBabyTouchGc.Server.Models;

namespace PoBabyTouchGc.Api.Middleware;

/// <summary>
/// Global exception handler middleware for consistent error handling
/// Applying Middleware Pattern for cross-cutting concerns
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        ApiResponse response;
        HttpStatusCode statusCode;

        switch (exception)
        {
            case ArgumentNullException:
            case ArgumentException:
                statusCode = HttpStatusCode.BadRequest;
                response = ApiResponse.CreateError("Invalid request parameters");
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                response = ApiResponse.CreateError("Unauthorized access");
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                response = ApiResponse.CreateError("Resource not found");
                break;

            case InvalidOperationException:
                statusCode = HttpStatusCode.Conflict;
                response = ApiResponse.CreateError("Operation conflict");
                break;

            // Handle Azure Table Storage specific exceptions
            case Azure.RequestFailedException azureEx:
                statusCode = HttpStatusCode.InternalServerError;
                response = ApiResponse.CreateError($"Azure Table Storage error: {azureEx.Message}");
                _logger.LogError(azureEx, "Azure Table Storage error: Status={Status}, ErrorCode={ErrorCode}",
                    azureEx.Status, azureEx.ErrorCode);
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                response = ApiResponse.CreateError("An internal server error occurred");
                _logger.LogError(exception, "Unhandled exception in API: {ExceptionType}", exception.GetType().Name);
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
