using PoBabyTouchGc.Api.Models;

namespace PoBabyTouchGc.Api.Features.HighScores;

/// <summary>
/// Validates high score submissions (Single Responsibility Principle).
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

        // Validate initials
        if (string.IsNullOrWhiteSpace(request.PlayerInitials))
            result.Errors.Add("Player initials cannot be empty");
        else if (request.PlayerInitials.Length != 3)
            result.Errors.Add("Player initials must be exactly 3 characters");
        else if (!request.PlayerInitials.All(char.IsLetterOrDigit))
            result.Errors.Add("Player initials must contain only letters and numbers");

        // Validate score
        if (request.Score < 0)
            result.Errors.Add("Score cannot be negative");
        else if (request.Score > 999999999)
            result.Errors.Add("Score exceeds maximum allowed value");

        // Validate game mode
        var gameMode = request.GameMode ?? "Default";
        if (string.IsNullOrWhiteSpace(gameMode))
            result.Errors.Add("Game mode cannot be empty");
        else if (gameMode.Length > 50)
            result.Errors.Add("Game mode cannot exceed 50 characters");
        else if (!gameMode.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'))
            result.Errors.Add("Game mode can only contain letters, numbers, underscores, and hyphens");

        _logger.LogDebug("Validation result: {IsValid}, Errors: {ErrorCount}",
            result.IsValid, result.Errors.Count);

        return result;
    }
}

public class ValidationResult
{
    public List<string> Errors { get; set; } = [];
    public bool IsValid => Errors.Count == 0;
    public string? ErrorMessage => Errors.FirstOrDefault();
}
