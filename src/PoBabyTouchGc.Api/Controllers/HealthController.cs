using Microsoft.AspNetCore.Mvc;
using Azure.Data.Tables;
using System.Net.NetworkInformation;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace PoBabyTouchGc.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly ILogger<HealthController> _logger;
    private readonly TelemetryConfiguration _telemetryConfiguration;

    public HealthController(
        TableServiceClient tableServiceClient,
        ILogger<HealthController> logger,
        TelemetryConfiguration telemetryConfiguration)
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

        // Check Azure Table Storage connectivity
        try
        {
            var tableClient = _tableServiceClient.GetTableClient("PoBabyTouchHighScores");
            await tableClient.CreateIfNotExistsAsync();

            // Attempt a query to verify read access
            var queryResults = tableClient.QueryAsync<TableEntity>(maxPerPage: 1);
            var firstPage = await queryResults.AsPages().GetAsyncEnumerator().MoveNextAsync();

            dependencies["azureTableStorage"] = new
            {
                status = "Healthy",
                tableName = "PoBabyTouchHighScores",
                message = "Table accessible and queryable"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure Table Storage health check failed");
            dependencies["azureTableStorage"] = new
            {
                status = "Unhealthy",
                error = ex.Message,
                type = ex.GetType().Name
            };
            overallStatus = "Unhealthy";
        }

        // Check Application Insights connectivity
        try
        {
            var connectionString = _telemetryConfiguration.ConnectionString;
            if (!string.IsNullOrEmpty(connectionString))
            {
                // Application Insights is configured
                dependencies["applicationInsights"] = new
                {
                    status = "Configured",
                    message = "Telemetry is enabled",
                    instrumentationKey = connectionString.Contains("InstrumentationKey=")
                        ? "***" + connectionString.Substring(connectionString.IndexOf("InstrumentationKey=") + 19, 8)
                        : "N/A"
                };
            }
            else
            {
                dependencies["applicationInsights"] = new
                {
                    status = "NotConfigured",
                    message = "Application Insights connection string not found"
                };
                // Not marking as unhealthy since AI is optional for basic functionality
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Application Insights health check failed");
            dependencies["applicationInsights"] = new
            {
                status = "Error",
                error = ex.Message
            };
        }

        // Check API itself
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
            Application = "PoBabyTouch",
            Version = "1.0.0",
            Dependencies = dependencies
        };

        return overallStatus == "Healthy" ? Ok(healthStatus) : StatusCode(503, healthStatus);
    }
}
