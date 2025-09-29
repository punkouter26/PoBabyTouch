using Microsoft.Extensions.Logging;
using PoBabyTouchGc.Shared.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace PoBabyTouchGc.Client.Services;

/// <summary>
/// Custom logger provider that sends client-side logs to the server endpoint
/// This enables centralized logging for Blazor WebAssembly applications
/// </summary>
public class ServerLoggerProvider : ILoggerProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _sessionId;
    private readonly bool _isEnabled;

    public ServerLoggerProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _sessionId = Guid.NewGuid().ToString();
        
        // Only enable server logging in development (based on LocalDebugging setting)
        _isEnabled = configuration.GetValue<bool>("LocalDebugging:Enabled", false);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new ServerLogger(_httpClient, categoryName, _sessionId, _isEnabled);
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}

/// <summary>
/// Custom logger that sends log entries to the server via HTTP POST
/// Implements structured logging with telemetry data for Application Insights
/// </summary>
public class ServerLogger : ILogger
{
    private readonly HttpClient _httpClient;
    private readonly string _categoryName;
    private readonly string _sessionId;
    private readonly bool _isEnabled;

    public ServerLogger(HttpClient httpClient, string categoryName, string sessionId, bool isEnabled)
    {
        _httpClient = httpClient;
        _categoryName = categoryName;
        _sessionId = sessionId;
        _isEnabled = isEnabled;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null; // Scopes not implemented for simplicity
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _isEnabled && logLevel >= LogLevel.Information;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        if (string.IsNullOrWhiteSpace(message))
            return;

        // Extract structured data from state if available
        var structuredData = new Dictionary<string, object>();
        
        if (state is IEnumerable<KeyValuePair<string, object?>> properties)
        {
            foreach (var prop in properties)
            {
                if (prop.Value != null)
                {
                    structuredData[prop.Key] = prop.Value;
                }
            }
        }

        // Add exception details if present
        if (exception != null)
        {
            structuredData["ExceptionType"] = exception.GetType().Name;
            structuredData["ExceptionMessage"] = exception.Message;
            structuredData["StackTrace"] = exception.StackTrace ?? "";
        }

        // Add performance and telemetry data
        structuredData["EventId"] = eventId.Id;
        structuredData["EventName"] = eventId.Name ?? "";
        structuredData["Timestamp"] = DateTime.UtcNow;
        structuredData["UserAgent"] = "Blazor WebAssembly";

        var logRequest = new ClientLogRequest
        {
            Message = message,
            Level = logLevel.ToString(),
            Timestamp = DateTime.UtcNow,
            Category = _categoryName,
            SessionId = _sessionId,
            Data = structuredData
        };

        // Send to server asynchronously without blocking
        _ = Task.Run(async () =>
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/log/client", logRequest);
                
                // Optional: Log to browser console if server logging fails
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to send log to server: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Fallback to browser console if server is unreachable
                Console.WriteLine($"Error sending log to server: {ex.Message}");
                Console.WriteLine($"Original log: [{logLevel}] {_categoryName}: {message}");
            }
        });
    }
}

/// <summary>
/// Extension methods for easy telemetry logging in Blazor components
/// </summary>
public static class TelemetryLoggerExtensions
{
    /// <summary>
    /// Log user interaction event with telemetry data
    /// </summary>
    public static void LogUserInteraction(this ILogger logger, string action, string? element = null, Dictionary<string, object>? additionalData = null)
    {
        var data = additionalData ?? new Dictionary<string, object>();
        data["Action"] = action;
        data["Element"] = element ?? "";
        data["InteractionTime"] = DateTime.UtcNow;
        
        logger.LogInformation("User interaction: {Action} on {Element}", action, element);
    }

    /// <summary>
    /// Log game performance metrics
    /// </summary>
    public static void LogPerformanceMetric(this ILogger logger, string metricName, double value, string? unit = null)
    {
        logger.LogInformation("Performance metric: {MetricName} = {Value} {Unit}", metricName, value, unit ?? "");
    }

    /// <summary>
    /// Log game session events
    /// </summary>
    public static void LogGameSession(this ILogger logger, string sessionEvent, string? gameMode = null, Dictionary<string, object>? sessionData = null)
    {
        var data = sessionData ?? new Dictionary<string, object>();
        data["SessionEvent"] = sessionEvent;
        data["GameMode"] = gameMode ?? "";
        data["SessionTime"] = DateTime.UtcNow;
        
        logger.LogInformation("Game session: {SessionEvent} in {GameMode}", sessionEvent, gameMode);
    }

    /// <summary>
    /// Log error with additional context
    /// </summary>
    public static void LogErrorWithContext(this ILogger logger, Exception exception, string context, Dictionary<string, object>? contextData = null)
    {
        var data = contextData ?? new Dictionary<string, object>();
        data["ErrorContext"] = context;
        data["ErrorTime"] = DateTime.UtcNow;
        
        logger.LogError(exception, "Error in {Context}: {Message}", context, exception.Message);
    }
}