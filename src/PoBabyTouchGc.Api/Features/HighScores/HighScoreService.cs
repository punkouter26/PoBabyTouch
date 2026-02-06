using PoBabyTouchGc.Api.Models;

namespace PoBabyTouchGc.Api.Features.HighScores;

/// <summary>
/// Service layer for high score business logic (Service Layer Pattern).
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
            _logger.LogDebug("Saving high score: {PlayerInitials} - {Score} in {GameMode}",
                playerInitials, score, gameMode);

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

            var highScore = new HighScore
            {
                PlayerInitials = playerInitials!,
                Score = score,
                GameMode = gameMode,
                ScoreDate = DateTime.UtcNow
            };

            await _repository.SaveHighScoreAsync(highScore);
            _logger.LogInformation("High score saved: {PlayerInitials} - {Score}", playerInitials, score);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save high score for {PlayerInitials}", playerInitials);
            return false;
        }
    }

    public async Task<List<HighScore>> GetTopScoresAsync(int count = 10, string gameMode = "Default")
    {
        _logger.LogDebug("Retrieving top {Count} scores for {GameMode}", count, gameMode);
        return await _repository.GetTopScoresAsync(count, gameMode);
    }

    public async Task<bool> IsHighScoreAsync(int score, string gameMode = "Default")
    {
        return await _repository.IsHighScoreAsync(score, gameMode);
    }

    public async Task<int> GetPlayerRankAsync(int score, string gameMode = "Default")
    {
        return await _repository.GetPlayerRankAsync(score, gameMode);
    }
}
