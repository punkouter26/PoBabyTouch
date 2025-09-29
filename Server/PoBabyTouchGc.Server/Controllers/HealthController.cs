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
        public ActionResult GetHealthStatus()
        {
            var healthStatus = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow
            };

            return Ok(healthStatus);
        }
    }
}
