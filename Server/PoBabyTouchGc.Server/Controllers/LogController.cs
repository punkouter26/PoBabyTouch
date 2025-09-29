using Microsoft.AspNetCore.Mvc;
using PoBabyTouchGc.Shared.Models;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace PoBabyTouchGc.Server.Controllers;

// KQL Queries for Azure Application Insights Dashboard:
/*
1. Client Error Rate Analysis
// Purpose: Monitor client-side errors and their frequency over time
// Usage: Create time chart widget in Azure Dashboard to track client error trends
traces
| where customDimensions.SourceContext == "ClientLogger"
| where severityLevel >= 3  // Warning level and above
| summarize ErrorCount = count() by bin(timestamp, 5m), severityLevel
| render timechart

2. Client Performance Metrics
// Purpose: Track client-side performance and user interaction patterns
// Usage: Create KPI widget to monitor user engagement metrics
traces
| where customDimensions.SourceContext == "ClientLogger"
| where message contains "Performance" or message contains "Timing"
| extend Duration = extractjson("$.duration", tostring(customDimensions))
| summarize AvgDuration = avg(toint(Duration)), Count = count() by bin(timestamp, 1h)
| render barchart

3. Client Session Analysis
// Purpose: Analyze user session data and game completion rates
// Usage: Create funnel chart to track user journey through the game
traces
| where customDimensions.SourceContext == "ClientLogger"
| where message contains "Session" or message contains "Game"
| extend SessionId = extractjson("$.sessionId", tostring(customDimensions))
| extend Action = extractjson("$.action", tostring(customDimensions))
| summarize UniqueActions = dcount(Action) by SessionId
| summarize Sessions = count() by UniqueActions
| render piechart

4. Real-time Client Monitoring
// Purpose: Live monitoring of client activities and health status
// Usage: Create live metrics widget for real-time monitoring
traces
| where customDimensions.SourceContext == "ClientLogger"
| where timestamp > ago(15m)
| summarize Count = count() by bin(timestamp, 1m), severityLevel
| render columnchart
*/

[ApiController]
[Route("api/[controller]")]
public class LogController : ControllerBase
{
    private readonly ILogger<LogController> _logger;

    public LogController(ILogger<LogController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Receives log messages from the Blazor WebAssembly client and writes them using the server's logger.
    /// This endpoint allows centralized logging of client-side events and errors.
    /// </summary>
    /// <param name="logRequest">The log message and metadata from the client</param>
    /// <returns>Success response if log was processed</returns>
    [HttpPost("client")]
    public ActionResult<ApiResponse<object>> LogClientMessage([FromBody] ClientLogRequest logRequest)
    {
        try
        {
            // Validate the log request
            if (logRequest == null || string.IsNullOrWhiteSpace(logRequest.Message))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid log request - message is required",
                    Data = null
                });
            }

            // Use Serilog's static logger with context for filtering
            var clientLogger = Log.ForContext("SourceContext", "ClientLogger")
                .ForContext("ClientTimestamp", logRequest.Timestamp)
                .ForContext("ClientLevel", logRequest.Level)
                .ForContext("ClientCategory", logRequest.Category ?? "General")
                .ForContext("SessionId", logRequest.SessionId)
                .ForContext("UserId", logRequest.UserId)
                .ForContext("GameMode", logRequest.GameMode)
                .ForContext("ClientData", logRequest.Data, destructureObjects: true);

            // Log the message with appropriate level
            switch (logRequest.Level.ToLowerInvariant())
            {
                case "debug":
                    clientLogger.Debug("CLIENT: {Message}", logRequest.Message);
                    break;
                case "information":
                case "info":
                    clientLogger.Information("CLIENT: {Message}", logRequest.Message);
                    break;
                case "warning":
                case "warn":
                    clientLogger.Warning("CLIENT: {Message}", logRequest.Message);
                    break;
                case "error":
                    clientLogger.Error("CLIENT: {Message}", logRequest.Message);
                    break;
                case "critical":
                case "fatal":
                    clientLogger.Fatal("CLIENT: {Message}", logRequest.Message);
                    break;
                default:
                    clientLogger.Information("CLIENT: {Message}", logRequest.Message);
                    break;
            }

            // Track telemetry for Application Insights using the standard logger
            _logger.LogInformation("Client log received: {Level} - {Category} - {Message}", 
                logRequest.Level, logRequest.Category, logRequest.Message);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Log message processed successfully",
                Data = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing client log request");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Internal server error while processing log",
                Data = null
            });
        }
    }
}