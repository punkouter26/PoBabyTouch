using Microsoft.AspNetCore.Mvc;
using PoBabyTouchGc.Api.Models;
using PoBabyTouchGc.Api.Features.HighScores;

namespace PoBabyTouchGc.Api.Controllers;

/// <summary>
/// API controller for high score CRUD operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HighScoresController : ControllerBase
{
    private readonly IHighScoreService _highScoreService;
    private readonly IHighScoreValidationService _validationService;
    private readonly ILogger<HighScoresController> _logger;

    public HighScoresController(
        IHighScoreService highScoreService,
        IHighScoreValidationService validationService,
        ILogger<HighScoresController> logger)
    {
        _highScoreService = highScoreService;
        _validationService = validationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<HighScore>>>> GetTopScores(
        [FromQuery] int count = 10,
        [FromQuery] string gameMode = "Default")
    {
        try
        {
            var scores = await _highScoreService.GetTopScoresAsync(count, gameMode);
            return Ok(ApiResponse<List<HighScore>>.SuccessResult(scores));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTopScores failed for {GameMode}", gameMode);
            return StatusCode(500, ApiResponse<List<HighScore>>.ErrorResult("Failed to retrieve top scores"));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> SaveHighScore(
        [FromBody] SaveHighScoreRequest request)
    {
        try
        {
            var validationResult = _validationService.ValidateHighScore(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed: {Error}", validationResult.ErrorMessage);
                return BadRequest(ApiResponse<object>.ErrorResult(
                    validationResult.ErrorMessage ?? "Validation Error"));
            }

            var success = await _highScoreService.SaveHighScoreAsync(
                request.PlayerInitials, request.Score, request.GameMode ?? "Default");

            return success
                ? Ok(ApiResponse<object>.SuccessResult(
                    new { message = "High score saved successfully" },
                    "High score saved successfully"))
                : StatusCode(500, ApiResponse<object>.ErrorResult("Failed to save high score"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving high score");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Internal server error"));
        }
    }

    [HttpGet("check/{score}")]
    public async Task<ActionResult<ApiResponse<bool>>> IsHighScore(
        int score, [FromQuery] string gameMode = "Default")
    {
        try
        {
            var isHighScore = await _highScoreService.IsHighScoreAsync(score, gameMode);
            return Ok(ApiResponse<bool>.SuccessResult(isHighScore));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check high score");
            return StatusCode(500, ApiResponse<bool>.ErrorResult("Failed to check high score"));
        }
    }

    [HttpGet("rank/{score}")]
    public async Task<ActionResult<ApiResponse<int>>> GetPlayerRank(
        int score, [FromQuery] string gameMode = "Default")
    {
        try
        {
            var rank = await _highScoreService.GetPlayerRankAsync(score, gameMode);
            return Ok(ApiResponse<int>.SuccessResult(rank));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get player rank");
            return StatusCode(500, ApiResponse<int>.ErrorResult("Failed to get player rank"));
        }
    }

    [HttpGet("test")]
    public async Task<ActionResult<ApiResponse<object>>> TestConnection()
    {
        try
        {
            var scores = await _highScoreService.GetTopScoresAsync(1, "Default");
            return Ok(ApiResponse<object>.SuccessResult(
                new { status = "connected", scoresCount = scores.Count }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "High score service test failed");
            return StatusCode(500, ApiResponse<object>.ErrorResult("Test failed"));
        }
    }

    [HttpGet("diagnostics")]
    public async Task<ActionResult<ApiResponse<object>>> GetDiagnostics()
    {
        try
        {
            await _highScoreService.GetTopScoresAsync(1, "Default");
            return Ok(ApiResponse<object>.SuccessResult(new
            {
                status = "running",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                connectionTest = "success"
            }));
        }
        catch (Exception ex)
        {
            return Ok(ApiResponse<object>.SuccessResult(new
            {
                status = "degraded",
                timestamp = DateTime.UtcNow,
                connectionTest = $"failed: {ex.Message}"
            }));
        }
    }
}
