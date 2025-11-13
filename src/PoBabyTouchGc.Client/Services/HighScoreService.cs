using PoBabyTouchGc.Shared.Models;

namespace PoBabyTouchGc.Client.Services;

/// <summary>
/// Centralized service for high score API operations
/// Applies DRY principle - eliminates duplicate HTTP code across 6+ components
/// </summary>
public class HighScoreService
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<HighScoreService> _logger;

    public HighScoreService(ApiClient apiClient, ILogger<HighScoreService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Get top high scores for a game mode
    /// </summary>
    public async Task<List<HighScore>> GetTopScoresAsync(string gameMode = "Default", int count = 10)
    {
        var response = await _apiClient.GetAsync<List<HighScore>>(
            $"/api/highscores?gameMode={gameMode}&count={count}");

        if (response.Success && response.Data != null)
        {
            return response.Data;
        }

        _logger.LogWarning("Failed to get high scores: {Message}", response.Message);
        return new List<HighScore>();
    }

    /// <summary>
    /// Submit a new high score
    /// </summary>
    public async Task<bool> SubmitScoreAsync(SaveHighScoreRequest request)
    {
        var response = await _apiClient.PostAsync("/api/highscores", request);
        
        if (!response.Success)
        {
            _logger.LogError("Failed to submit score: {Message}", response.Message);
        }

        return response.Success;
    }

    /// <summary>
    /// Check if a score qualifies as a high score
    /// </summary>
    public async Task<bool> IsHighScoreAsync(int score, string gameMode = "Default")
    {
        var response = await _apiClient.GetAsync<bool>(
            $"/api/highscores/check/{score}?gameMode={gameMode}");

        return response.Success && response.Data;
    }

    /// <summary>
    /// Get player rank for a score
    /// </summary>
    public async Task<int> GetPlayerRankAsync(int score, string gameMode = "Default")
    {
        var response = await _apiClient.GetAsync<int>(
            $"/api/highscores/rank/{score}?gameMode={gameMode}");

        return response.Success ? response.Data : -1;
    }
}
