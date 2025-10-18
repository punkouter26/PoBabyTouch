using Microsoft.AspNetCore.Mvc;
using PoBabyTouchGc.Shared.Models;
using PoBabyTouchGc.Server.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using HighScore = PoBabyTouchGc.Shared.Models.HighScore; // Use the shared model for API responses

namespace PoBabyTouchGc.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HighScoresController : ControllerBase
    {
        private readonly IHighScoreService _highScoreService;
        private readonly IHighScoreValidationService _validationService;
        private readonly ILogger<HighScoresController> _logger;
        private readonly TelemetryClient _telemetryClient;

        public HighScoresController(
            IHighScoreService highScoreService,
            IHighScoreValidationService validationService,
            ILogger<HighScoresController> logger,
            TelemetryClient telemetryClient)
        {
            _highScoreService = highScoreService;
            _validationService = validationService;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Get top high scores
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<HighScore>>>> GetTopScores(
            [FromQuery] int count = 10,
            [FromQuery] string gameMode = "Default")
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogDebug("Getting top {Count} scores for {GameMode} mode", count, gameMode);

                // Application Insights: Track custom event for leaderboard views
                _telemetryClient.TrackEvent("LeaderboardViewed", new Dictionary<string, string>
                {
                    { "GameMode", gameMode },
                    { "Count", count.ToString() },
                    { "UserAgent", Request.Headers["User-Agent"].ToString() }
                });

                var scores = await _highScoreService.GetTopScoresAsync(count, gameMode);

                // Telemetry: Track performance and results
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _telemetryClient.TrackMetric("LeaderboardLoadTime", duration, new Dictionary<string, string>
                {
                    { "GameMode", gameMode },
                    { "ScoreCount", scores.Count.ToString() }
                });
                
                _logger.LogInformation("API Performance: GetTopScores completed in {Duration}ms, returned {ScoreCount} scores",
                    duration, scores.Count);

                return Ok(ApiResponse<List<HighScore>>.SuccessResult(scores));
            }
            catch (Exception ex)
            {
                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogError(ex, "API Error: GetTopScores failed after {Duration}ms for GameMode: {GameMode}",
                    duration, gameMode);
                return StatusCode(500, ApiResponse<List<HighScore>>.ErrorResult("Failed to retrieve top scores"));
            }
        }

        /// <summary>
        /// Save a new high score
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> SaveHighScore([FromBody] SaveHighScoreRequest request)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                _logger.LogDebug("Saving high score: {PlayerInitials} - {Score} points",
                    request.PlayerInitials, request.Score);

                // Application Insights: Track high score submission attempt
                _telemetryClient.TrackEvent("HighScoreSubmitted", new Dictionary<string, string>
                {
                    { "PlayerInitials", request.PlayerInitials },
                    { "GameMode", request.GameMode ?? "Default" },
                    { "UserAgent", Request.Headers["User-Agent"].ToString() }
                }, new Dictionary<string, double>
                {
                    { "Score", request.Score }
                });

                // Use validation service instead of inline validation
                var validationResult = _validationService.ValidateHighScore(request);

                if (!validationResult.IsValid)
                {
                    // Telemetry: Track validation failures
                    _telemetryClient.TrackEvent("HighScoreValidationFailed", new Dictionary<string, string>
                    {
                        { "PlayerInitials", request.PlayerInitials },
                        { "ValidationError", validationResult.ErrorMessage ?? "Unknown" },
                        { "GameMode", request.GameMode ?? "Default" }
                    }, new Dictionary<string, double>
                    {
                        { "Score", request.Score }
                    });
                    
                    _logger.LogWarning("HighScore Validation Failed: Player: {PlayerInitials}, Score: {Score}, Error: {ValidationError}",
                        request.PlayerInitials, request.Score, validationResult.ErrorMessage);

                    var validationResponse = ApiResponse<object>.ErrorResult(validationResult.ErrorMessage ?? "Validation Error");
                    return BadRequest(validationResponse);
                }

                var success = await _highScoreService.SaveHighScoreAsync(
                    request.PlayerInitials,
                    request.Score,
                    request.GameMode ?? "Default");

                var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

                if (success)
                {
                    // Application Insights: Track successful high score saves
                    _telemetryClient.TrackEvent("HighScoreSaved", new Dictionary<string, string>
                    {
                        { "PlayerInitials", request.PlayerInitials },
                        { "GameMode", request.GameMode ?? "Default" }
                    }, new Dictionary<string, double>
                    {
                        { "Score", request.Score },
                        { "SaveDurationMs", duration }
                    });
                    
                    _logger.LogInformation("HighScore Success: Player: {PlayerInitials}, Score: {Score}, GameMode: {GameMode}, Duration: {Duration}ms",
                        request.PlayerInitials, request.Score, request.GameMode, duration);

                    var response = ApiResponse<object>.SuccessResult(new { message = "High score saved successfully" }, "High score saved successfully");
                    return Ok(response);
                }
                else
                {
                    // Telemetry: Track failed saves
                    _logger.LogError("HighScore Save Failed: Player: {PlayerInitials}, Score: {Score}, GameMode: {GameMode}, Duration: {Duration}ms",
                        request.PlayerInitials, request.Score, request.GameMode, duration);

                    var response = ApiResponse<object>.ErrorResult("Failed to save high score");
                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving high score");
                var response = ApiResponse<object>.ErrorResult("Internal server error");
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Check if a score qualifies as a high score
        /// </summary>
        [HttpGet("check/{score}")]
        public async Task<ActionResult<ApiResponse<bool>>> IsHighScore(int score, [FromQuery] string gameMode = "Default")
        {
            try
            {
                _logger.LogDebug("Checking if score {Score} is a high score", score);

                var isHighScore = await _highScoreService.IsHighScoreAsync(score, gameMode);
                return Ok(ApiResponse<bool>.SuccessResult(isHighScore));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if score is high score");
                return StatusCode(500, ApiResponse<bool>.ErrorResult("Failed to check if score is high score"));
            }
        }

        /// <summary>
        /// Get player rank for a given score
        /// </summary>
        [HttpGet("rank/{score}")]
        public async Task<ActionResult<ApiResponse<int>>> GetPlayerRank(int score, [FromQuery] string gameMode = "Default")
        {
            try
            {
                _logger.LogDebug("Getting rank for score {Score}", score);

                var rank = await _highScoreService.GetPlayerRankAsync(score, gameMode);
                return Ok(ApiResponse<int>.SuccessResult(rank));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get player rank");
                return StatusCode(500, ApiResponse<int>.ErrorResult("Failed to get player rank"));
            }
        }

        /// <summary>
        /// Test endpoint for diagnostics - checks if the service is working
        /// </summary>
        [HttpGet("test")]
        public async Task<ActionResult<ApiResponse<object>>> TestConnection()
        {
            try
            {
                _logger.LogDebug("Testing high score service connection");

                // Try to get top scores as a simple test
                var scores = await _highScoreService.GetTopScoresAsync(1, "Default");
                var testData = new { status = "connected", message = "High score service is working", scoresCount = scores.Count };
                var response = ApiResponse<object>.SuccessResult(testData, "Service test completed successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "High score service test failed");
                var response = ApiResponse<object>.ErrorResult("High score service test failed");
                return StatusCode(500, response);
            }
        }

        /// <summary>
        /// Diagnostic endpoint for troubleshooting Azure Table Storage connection
        /// </summary>
        [HttpGet("diagnostics")]
        public async Task<ActionResult<ApiResponse<object>>> GetDiagnostics()
        {
            try
            {
                _logger.LogDebug("Running diagnostics for high score service");

                var diagnosticInfo = new
                {
                    status = "running",
                    timestamp = DateTime.UtcNow,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    connectionTest = "attempting...",
                    tableTest = "pending...",
                    serviceTest = "pending..."
                };

                // Test connection by attempting to get service properties
                try
                {
                    await _highScoreService.GetTopScoresAsync(1, "Default");
                    diagnosticInfo = diagnosticInfo with
                    {
                        connectionTest = "success",
                        tableTest = "success",
                        serviceTest = "success"
                    };
                }
                catch (Exception serviceEx)
                {
                    _logger.LogError(serviceEx, "High score service diagnostic test failed");
                    diagnosticInfo = diagnosticInfo with
                    {
                        connectionTest = "failed",
                        tableTest = "failed",
                        serviceTest = $"failed: {serviceEx.Message}"
                    };
                }

                var response = ApiResponse<object>.SuccessResult(diagnosticInfo, "Diagnostics completed");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Diagnostics endpoint failed");
                var response = ApiResponse<object>.ErrorResult($"Diagnostics failed: {ex.Message}");
                return StatusCode(500, response);
            }
        }
    }
}

/* ==================== APPLICATION INSIGHTS KQL QUERIES ====================
 * Use these queries in Azure Portal > Application Insights > Logs
 * 
 * QUERY 1: High Score Submissions by Player and Game Mode (Last 7 Days)
 * -----------------------------------------------------------------------
 * Shows which players are most active and their score distribution
 * 
 * customEvents
 * | where timestamp > ago(7d)
 * | where name == "HighScoreSaved"
 * | extend PlayerInitials = tostring(customDimensions.PlayerInitials),
 *          GameMode = tostring(customDimensions.GameMode),
 *          Score = todouble(customMeasurements.Score)
 * | summarize SubmissionCount = count(), 
 *             AvgScore = avg(Score), 
 *             MaxScore = max(Score),
 *             MinScore = min(Score) by PlayerInitials, GameMode
 * | order by SubmissionCount desc
 * 
 * 
 * QUERY 2: Leaderboard Performance Metrics (Last 24 Hours)
 * ---------------------------------------------------------
 * Tracks API response times and usage patterns
 * 
 * customMetrics
 * | where timestamp > ago(24h)
 * | where name == "LeaderboardLoadTime"
 * | extend GameMode = tostring(customDimensions.GameMode),
 *          ScoreCount = tostring(customDimensions.ScoreCount)
 * | summarize RequestCount = count(),
 *             AvgLoadTime = avg(value),
 *             P50 = percentile(value, 50),
 *             P95 = percentile(value, 95),
 *             P99 = percentile(value, 99) by GameMode
 * | order by RequestCount desc
 * 
 * 
 * QUERY 3: Validation Failures and Error Analysis
 * ------------------------------------------------
 * Identifies common validation issues and potential cheating attempts
 * 
 * customEvents
 * | where timestamp > ago(7d)
 * | where name == "HighScoreValidationFailed"
 * | extend PlayerInitials = tostring(customDimensions.PlayerInitials),
 *          ValidationError = tostring(customDimensions.ValidationError),
 *          Score = todouble(customMeasurements.Score)
 * | summarize FailureCount = count(),
 *             AvgInvalidScore = avg(Score),
 *             MaxInvalidScore = max(Score) by ValidationError
 * | order by FailureCount desc
 * 
 * ==================== END KQL QUERIES ====================
 */
