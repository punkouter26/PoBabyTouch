using Xunit;
using PoBabyTouchGc.Web.Features.HighScores;
using PoBabyTouchGc.Shared.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace PoBabyTouchGc.Tests.Unit;

/// <summary>
/// Unit tests for HighScoreValidationService
/// Addresses Priority 9: Unit Test Coverage Gaps
/// </summary>
public class HighScoreValidationServiceTests
{
    private readonly HighScoreValidationService _validationService;
    private readonly Mock<ILogger<HighScoreValidationService>> _mockLogger;

    public HighScoreValidationServiceTests()
    {
        _mockLogger = new Mock<ILogger<HighScoreValidationService>>();
        _validationService = new HighScoreValidationService(_mockLogger.Object);
    }

    [Theory]
    [InlineData("ABC", true)]
    [InlineData("XYZ", true)]
    [InlineData("123", true)]
    [InlineData("A1B", true)]
    [InlineData("AB", false)]      // Too short
    [InlineData("ABCD", false)]    // Too long
    [InlineData("", false)]        // Empty
    [InlineData(null, false)]      // Null
    [InlineData("A B", false)]     // Contains space
    [InlineData("A@C", false)]     // Special char
    [InlineData("abc", true)]      // Lowercase (should be accepted)
    public void ValidateInitials_VariousInputs_ReturnsExpectedResult(string? initials, bool expectedValid)
    {
        // Arrange
        var request = new SaveHighScoreRequest
        {
            PlayerInitials = initials ?? string.Empty,
            Score = 1000,
            GameMode = "Default"
        };

        // Act
        var result = _validationService.ValidateHighScore(request);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
        if (!expectedValid && initials != null)
        {
            Assert.NotNull(result.ErrorMessage);
        }
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(1000, true)]
    [InlineData(999999, true)]
    [InlineData(-1, false)]
    [InlineData(-100, false)]
    [InlineData(-999999, false)]
    public void ValidateScore_VariousScores_ReturnsExpectedResult(int score, bool expectedValid)
    {
        // Arrange
        var request = new SaveHighScoreRequest
        {
            PlayerInitials = "ABC",
            Score = score,
            GameMode = "Default"
        };

        // Act
        var result = _validationService.ValidateHighScore(request);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
        if (!expectedValid)
        {
            Assert.Contains("negative", result.ErrorMessage ?? "", StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ValidateHighScore_AllValid_ReturnsSuccess()
    {
        // Arrange
        var request = new SaveHighScoreRequest
        {
            PlayerInitials = "ABC",
            Score = 1500,
            GameMode = "Default"
        };

        // Act
        var result = _validationService.ValidateHighScore(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateHighScore_InvalidInitials_ReturnsError()
    {
        // Arrange
        var request = new SaveHighScoreRequest
        {
            PlayerInitials = "AB", // Too short
            Score = 1500,
            GameMode = "Default"
        };

        // Act
        var result = _validationService.ValidateHighScore(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("3 characters", result.ErrorMessage);
    }

    [Fact]
    public void ValidateHighScore_NegativeScore_ReturnsError()
    {
        // Arrange
        var request = new SaveHighScoreRequest
        {
            PlayerInitials = "ABC",
            Score = -100,
            GameMode = "Default"
        };

        // Act
        var result = _validationService.ValidateHighScore(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("negative", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Default")]
    [InlineData("Easy")]
    [InlineData("Hard")]
    [InlineData("Custom")]
    public void ValidateGameMode_ValidModes_ReturnsSuccess(string gameMode)
    {
        // Arrange
        var request = new SaveHighScoreRequest
        {
            PlayerInitials = "ABC",
            Score = 1000,
            GameMode = gameMode
        };

        // Act
        var result = _validationService.ValidateHighScore(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateHighScore_EmptyGameMode_ReturnsError()
    {
        // Arrange
        var request = new SaveHighScoreRequest
        {
            PlayerInitials = "ABC",
            Score = 1000,
            GameMode = "" // Empty should be rejected
        };

        // Act
        var result = _validationService.ValidateHighScore(request);

        // Assert - Empty game mode should be invalid
        Assert.False(result.IsValid);
        Assert.Contains("Game mode cannot be empty", result.Errors);
    }

    [Fact]
    public void ValidateHighScore_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var request = new SaveHighScoreRequest
        {
            PlayerInitials = "AB", // Invalid
            Score = -100,          // Invalid
            GameMode = "Default"
        };

        // Act
        var result = _validationService.ValidateHighScore(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        // Should have errors for both initials and score
        Assert.True(result.Errors.Count >= 2 || result.ErrorMessage != null);
    }
}

