using Microsoft.AspNetCore.Mvc;
using PoBabyTouchGc.Shared.Models;

namespace PoBabyTouchGc.Api.Features.Common;

/// <summary>
/// Helper class for creating consistent API responses
/// Applies DRY principle and Factory pattern
/// Eliminates duplicate response creation code across controllers
/// </summary>
public static class ApiResponseHelper
{
    /// <summary>
    /// Create a successful response with data
    /// </summary>
    public static ActionResult<ApiResponse<T>> Success<T>(T data, string message = "Success")
    {
        return new OkObjectResult(ApiResponse<T>.SuccessResult(data, message));
    }

    /// <summary>
    /// Create an error response with custom status code
    /// </summary>
    public static ActionResult<ApiResponse<T>> Error<T>(string message, int statusCode = 500)
    {
        return new ObjectResult(ApiResponse<T>.ErrorResult(message))
        {
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Create a bad request (400) error response
    /// </summary>
    public static ActionResult<ApiResponse<T>> BadRequest<T>(string message)
    {
        return new BadRequestObjectResult(ApiResponse<T>.ErrorResult(message));
    }

    /// <summary>
    /// Create a not found (404) error response
    /// </summary>
    public static ActionResult<ApiResponse<T>> NotFound<T>(string message)
    {
        return new NotFoundObjectResult(ApiResponse<T>.ErrorResult(message));
    }

    /// <summary>
    /// Create an unauthorized (401) error response
    /// </summary>
    public static ActionResult<ApiResponse<T>> Unauthorized<T>(string message)
    {
        return new UnauthorizedObjectResult(ApiResponse<T>.ErrorResult(message));
    }
}
