using Microsoft.AspNetCore.Mvc;
using Azure.Data.Tables;
using System.Net.NetworkInformation;

namespace PoBabyTouchGc.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly ILogger<HealthController> _logger;

        public HealthController(TableServiceClient tableServiceClient, ILogger<HealthController> logger)
        {
            _tableServiceClient = tableServiceClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetHealthStatus()
        {
            var dependencies = new Dictionary<string, object>();
            var overallStatus = "Healthy";

            try
            {
                // Check Azure Table Storage connectivity
                var tableClient = _tableServiceClient.GetTableClient("PoBabyTouchGcHighScores");
                await tableClient.CreateIfNotExistsAsync();
                dependencies["tableStorage"] = new { status = "Healthy" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure Table Storage health check failed");
                dependencies["tableStorage"] = new { status = "Unhealthy", error = ex.Message };
                overallStatus = "Unhealthy";
            }

            var healthStatus = new
            {
                Status = overallStatus,
                Timestamp = DateTime.UtcNow,
                Dependencies = dependencies
            };

            return Ok(healthStatus);
        }
    }
}
