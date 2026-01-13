using PoBabyTouchGc.Shared.Models;

namespace PoBabyTouchGc.Web.Features.HighScores;

/// <summary>
/// Validation service for high score data
/// Applying Single Responsibility Principle - dedicated to validation logic
/// Follows Strategy Pattern for different validation strategies
/// </summary>
public interface IHighScoreValidationService
{
    ValidationResult ValidateHighScore(SaveHighScoreRequest request);
}

public class HighScoreValidationService : IHighScoreValidationService
{
    private readonly ILogger<HighScoreValidationService> _logger;

    public HighScoreValidationService(ILogger<HighScoreValidationService> logger)
    {
        _logger = logger;
    }

    public ValidationResult ValidateHighScore(SaveHighScoreRequest request)
    {
        var result = new ValidationResult();

        // Validate player initials
        var initialsResult = ValidatePlayerInitials(request.PlayerInitials);
        if (!initialsResult.IsValid)
        {
            result.Errors.AddRange(initialsResult.Errors);
        }

        // Validate score
        var scoreResult = ValidateScore(request.Score);
        if (!scoreResult.IsValid)
        {
            result.Errors.AddRange(scoreResult.Errors);
        }

        // Validate game mode
        var gameModeResult = ValidateGameMode(request.GameMode ?? "Default");
        if (!gameModeResult.IsValid)
        {
            result.Errors.AddRange(gameModeResult.Errors);
        }

        _logger.LogDebug("Validation result for high score: {IsValid}, Errors: {ErrorCount}",
            result.IsValid, result.Errors.Count);

        return result;
    }



    public ValidationResult ValidatePlayerInitials(string playerInitials)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(playerInitials))
        {
            result.Errors.Add("Player initials cannot be empty");
        }
        else if (playerInitials.Length != 3)
        {
            result.Errors.Add("Player initials must be exactly 3 characters");
        }
        else if (!playerInitials.All(char.IsLetterOrDigit))
        {
            result.Errors.Add("Player initials must contain only letters and numbers");
        }

        return result;
    }

    public ValidationResult ValidateScore(int score)
    {
        var result = new ValidationResult();

        if (score < 0)
        {
            result.Errors.Add("Score cannot be negative");
        }
        else if (score > 999999999) // Reasonable maximum
        {
            result.Errors.Add("Score exceeds maximum allowed value");
        }

        return result;
    }

    public ValidationResult ValidateGameMode(string gameMode)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(gameMode))
        {
            result.Errors.Add("Game mode cannot be empty");
        }
        else if (gameMode.Length > 50)
        {
            result.Errors.Add("Game mode cannot exceed 50 characters");
        }
        else if (!gameMode.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'))
        {
            result.Errors.Add("Game mode can only contain letters, numbers, underscores, and hyphens");
        }

        return result;
    }
}

/// <summary>
/// Validation result container
/// Applying Value Object Pattern for validation results
/// </summary>
public class ValidationResult
{
    public List<string> Errors { get; set; } = new();
    public bool IsValid => Errors.Count == 0;
    public string? ErrorMessage => Errors.FirstOrDefault();
}
