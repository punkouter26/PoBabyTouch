using Microsoft.AspNetCore.Mvc;
using Azure.Data.Tables;
using Microsoft.ApplicationInsights.Extensibility;

namespace PoBabyTouchGc.Api.Controllers;

/// <summary>
/// Health check controller — verifies connectivity to all dependencies.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly TableServiceClient? _tableServiceClient;
    private readonly ILogger<HealthController> _logger;
    private readonly TelemetryConfiguration? _telemetryConfiguration;

    public HealthController(
        ILogger<HealthController> logger,
        TableServiceClient? tableServiceClient = null,
        TelemetryConfiguration? telemetryConfiguration = null)
    {
        _tableServiceClient = tableServiceClient;
        _logger = logger;
        _telemetryConfiguration = telemetryConfiguration;
    }

    [HttpGet]
    public async Task<ActionResult> GetHealthStatus()
    {
        var dependencies = new Dictionary<string, object>();
        var overallStatus = "Healthy";

        // Check Azure Table Storage
        if (_tableServiceClient != null)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient("PoBabyTouchHighScores");
                await tableClient.CreateIfNotExistsAsync();
                dependencies["azureTableStorage"] = new { status = "Healthy", message = "Table accessible" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure Table Storage health check failed");
                dependencies["azureTableStorage"] = new { status = "Unhealthy", error = ex.Message };
                overallStatus = "Degraded";
            }
        }
        else
        {
            dependencies["azureTableStorage"] = new { status = "NotConfigured", message = "No connection string" };
        }

        // Check Application Insights
        var aiConnectionString = _telemetryConfiguration?.ConnectionString;
        dependencies["applicationInsights"] = !string.IsNullOrEmpty(aiConnectionString)
            ? new { status = "Configured", message = "Telemetry enabled" }
            : (object)new { status = "NotConfigured" };

        dependencies["api"] = new
        {
            status = "Healthy",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            timestamp = DateTime.UtcNow,
            machineName = Environment.MachineName
        };

        var healthStatus = new
        {
            Status = overallStatus,
            Timestamp = DateTime.UtcNow,
            Application = "PoBabyTouchGc",
            Version = "2.0.0",
            Dependencies = dependencies
        };

        return overallStatus == "Healthy" ? Ok(healthStatus) : StatusCode(503, healthStatus);
    }
}
