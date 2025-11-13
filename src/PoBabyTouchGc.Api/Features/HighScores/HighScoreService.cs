using PoBabyTouchGc.Shared.Models;

namespace PoBabyTouchGc.Api.Features.HighScores;

/// <summary>
/// Service for managing high scores
/// Applying Service Layer Pattern for business logic abstraction
/// Uses Repository Pattern for data access abstraction
/// Follows SOLID principles - Single Responsibility and Dependency Inversion
/// </summary>
public interface IHighScoreService
{
    Task<bool> SaveHighScoreAsync(string? playerInitials, int score, string gameMode = "Default");
    Task<List<HighScore>> GetTopScoresAsync(int count = 10, string gameMode = "Default");
    Task<bool> IsHighScoreAsync(int score, string gameMode = "Default");
    Task<int> GetPlayerRankAsync(int score, string gameMode = "Default");
}

public class HighScoreService : IHighScoreService
{
    private readonly IHighScoreRepository _repository;
    private readonly IHighScoreValidationService _validationService;
    private readonly ILogger<HighScoreService> _logger;

    public HighScoreService(
        IHighScoreRepository repository,
        IHighScoreValidationService validationService,
        ILogger<HighScoreService> logger)
    {
        _repository = repository;
        _validationService = validationService;
        _logger = logger;
    }

    public async Task<bool> SaveHighScoreAsync(string? playerInitials, int score, string gameMode = "Default")
    {
        try
        {
            _logger.LogDebug("Saving high score: {PlayerInitials} - {Score} points in {GameMode} mode",
                playerInitials, score, gameMode);

            // Validate input using validation service
            var validationResult = _validationService.ValidateHighScore(new SaveHighScoreRequest
            {
                PlayerInitials = playerInitials ?? string.Empty,
                Score = score,
                GameMode = gameMode
            });

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid high score data: {Errors}",
                    string.Join(", ", validationResult.Errors));
                return false;
            }

            // Create high score entity
            var highScore = new HighScore
            {
                PlayerInitials = playerInitials!, // Null-forgiving operator used after validation
                Score = score,
                GameMode = gameMode,
                ScoreDate = DateTime.UtcNow
            };

            // Save using repository
            await _repository.SaveHighScoreAsync(highScore);

            _logger.LogInformation("High score saved successfully: {PlayerInitials} - {Score} points",
                playerInitials, score);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save high score for {PlayerInitials} - {Score} points",
                playerInitials, score);
            return false;
        }
    }

    public async Task<List<HighScore>> GetTopScoresAsync(int count = 10, string gameMode = "Default")
    {
        try
        {
            _logger.LogDebug("Retrieving top {Count} scores for {GameMode} mode", count, gameMode);

            var scores = await _repository.GetTopScoresAsync(count, gameMode);

            _logger.LogInformation("Retrieved {Count} high scores for {GameMode} mode",
                scores.Count, gameMode);
            return scores;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve high scores for {GameMode} mode", gameMode);
            throw;
        }
    }

    public async Task<bool> IsHighScoreAsync(int score, string gameMode = "Default")
    {
        try
        {
            var isHighScore = await _repository.IsHighScoreAsync(score, gameMode);

            _logger.LogDebug("Score {Score} is {IsHighScore} a high score for {GameMode} mode",
                score, isHighScore ? "" : "not", gameMode);
            return isHighScore;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if score {Score} is a high score", score);
            throw;
        }
    }

    public async Task<int> GetPlayerRankAsync(int score, string gameMode = "Default")
    {
        try
        {
            var rank = await _repository.GetPlayerRankAsync(score, gameMode);

            _logger.LogDebug("Score {Score} ranks #{Rank} for {GameMode} mode",
                score, rank, gameMode);
            return rank;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get player rank for score {Score}", score);
            throw;
        }
    }
}
