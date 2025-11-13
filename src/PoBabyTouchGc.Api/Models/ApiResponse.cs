using System.Text.Json.Serialization;

namespace PoBabyTouchGc.Server.Models
{
    /// <summary>
    /// Standard API response wrapper for consistent response format
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("errors")]
        public List<string>? Errors { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public ApiResponse()
        {
        }

        public ApiResponse(T data, string? message = null)
        {
            Success = true;
            Data = data;
            Message = message;
        }

        public ApiResponse(string errorMessage)
        {
            Success = false;
            Errors = new List<string> { errorMessage };
        }

        public ApiResponse(List<string> errors)
        {
            Success = false;
            Errors = errors;
        }

        // Static factory methods
        public static ApiResponse<T> CreateSuccess(T data, string? message = null)
        {
            return new ApiResponse<T>(data, message);
        }

        public static ApiResponse<T> CreateError(string errorMessage)
        {
            return new ApiResponse<T>(errorMessage);
        }

        public static ApiResponse<T> CreateError(List<string> errors)
        {
            return new ApiResponse<T>(errors);
        }
    }

    /// <summary>
    /// API response without data payload
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        public ApiResponse() : base() { }

        public ApiResponse(string? message) : base()
        {
            Success = true;
            Message = message;
        }

        public ApiResponse(string errorMessage, bool isError) : base()
        {
            Success = !isError;
            if (isError)
            {
                Errors = new List<string> { errorMessage };
            }
            else
            {
                Message = errorMessage;
            }
        }

        public ApiResponse(List<string> errors) : base(errors) { }

        // Static factory methods
        public static ApiResponse CreateSuccess(string? message = null)
        {
            return new ApiResponse(message ?? "Operation completed successfully");
        }

        public static new ApiResponse CreateError(string errorMessage)
        {
            return new ApiResponse(errorMessage, true);
        }

        public static new ApiResponse CreateError(List<string> errors)
        {
            return new ApiResponse(errors);
        }
    }
}
