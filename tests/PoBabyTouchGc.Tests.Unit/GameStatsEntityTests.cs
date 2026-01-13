using Xunit;
using PoBabyTouchGc.Shared.Models;

namespace PoBabyTouchGc.Tests.Unit;

/// <summary>
/// Unit tests for GameStatsEntity mapping
/// Addresses Priority 9: Unit Test Coverage Gaps
/// </summary>
public class GameStatsEntityTests
{
    [Fact]
    public void ToGameStats_ValidEntity_MapsAllProperties()
    {
        // Arrange
        var entity = new GameStatsEntity
        {
            PartitionKey = "GameStats",
            RowKey = "ABC",
            Initials = "ABC", // Must match RowKey for proper mapping
            TotalGames = 10,
            HighestScore = 2500,
            AverageScore = 1800.5,
            TotalCirclesTapped = 500,
            TotalPlaytimeSeconds = 1200,
            PercentileRank = 75.5,
            ScoreDistribution = "0-99:2,100-199:5,200-299:3",
            FirstPlayed = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            LastPlayed = new DateTime(2025, 11, 12, 15, 30, 0, DateTimeKind.Utc)
        };

        // Act
        var gameStats = entity.ToGameStats();

        // Assert
        Assert.NotNull(gameStats);
        Assert.Equal("ABC", gameStats.Initials);
        Assert.Equal(10, gameStats.TotalGames);
        Assert.Equal(2500, gameStats.HighestScore);
        Assert.Equal(1800.5, gameStats.AverageScore);
        Assert.Equal(500, gameStats.TotalCirclesTapped);
        Assert.Equal(1200, gameStats.TotalPlaytimeSeconds);
        Assert.Equal(75.5, gameStats.PercentileRank);
        Assert.Equal("0-99:2,100-199:5,200-299:3", gameStats.ScoreDistribution);
        Assert.Equal(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc), gameStats.FirstPlayed);
        Assert.Equal(new DateTime(2025, 11, 12, 15, 30, 0, DateTimeKind.Utc), gameStats.LastPlayed);
    }

    [Fact]
    public void FromGameStats_ValidModel_CreatesCorrectEntity()
    {
        // Arrange
        var gameStats = new GameStats
        {
            Initials = "XYZ",
            TotalGames = 5,
            HighestScore = 1500,
            AverageScore = 1200.0,
            TotalCirclesTapped = 250,
            TotalPlaytimeSeconds = 600,
            PercentileRank = 50.0,
            ScoreDistribution = "0-99:1,100-199:3,200-299:1",
            FirstPlayed = new DateTime(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            LastPlayed = new DateTime(2025, 11, 12, 14, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var entity = GameStatsEntity.FromGameStats(gameStats);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal("GameStats", entity.PartitionKey);
        Assert.Equal("XYZ", entity.RowKey);
        Assert.Equal(5, entity.TotalGames);
        Assert.Equal(1500, entity.HighestScore);
        Assert.Equal(1200.0, entity.AverageScore);
        Assert.Equal(250, entity.TotalCirclesTapped);
        Assert.Equal(600, entity.TotalPlaytimeSeconds);
        Assert.Equal(50.0, entity.PercentileRank);
        Assert.Equal("0-99:1,100-199:3,200-299:1", entity.ScoreDistribution);
        Assert.Equal(new DateTime(2025, 6, 1, 10, 0, 0, DateTimeKind.Utc), entity.FirstPlayed);
        Assert.Equal(new DateTime(2025, 11, 12, 14, 0, 0, DateTimeKind.Utc), entity.LastPlayed);
    }

    [Fact]
    public void ToGameStats_FromGameStats_RoundTrip_PreservesData()
    {
        // Arrange
        var originalStats = new GameStats
        {
            Initials = "RTT",
            TotalGames = 15,
            HighestScore = 3000,
            AverageScore = 2250.75,
            TotalCirclesTapped = 750,
            TotalPlaytimeSeconds = 1800,
            PercentileRank = 90.5,
            ScoreDistribution = "0-99:3,100-199:7,200-299:5",
            FirstPlayed = new DateTime(2025, 3, 15, 8, 30, 0, DateTimeKind.Utc),
            LastPlayed = new DateTime(2025, 11, 12, 16, 45, 0, DateTimeKind.Utc)
        };

        // Act
        var entity = GameStatsEntity.FromGameStats(originalStats);
        var roundTrippedStats = entity.ToGameStats();

        // Assert
        Assert.Equal(originalStats.Initials, roundTrippedStats.Initials);
        Assert.Equal(originalStats.TotalGames, roundTrippedStats.TotalGames);
        Assert.Equal(originalStats.HighestScore, roundTrippedStats.HighestScore);
        Assert.Equal(originalStats.AverageScore, roundTrippedStats.AverageScore);
        Assert.Equal(originalStats.TotalCirclesTapped, roundTrippedStats.TotalCirclesTapped);
        Assert.Equal(originalStats.TotalPlaytimeSeconds, roundTrippedStats.TotalPlaytimeSeconds);
        Assert.Equal(originalStats.PercentileRank, roundTrippedStats.PercentileRank);
        Assert.Equal(originalStats.ScoreDistribution, roundTrippedStats.ScoreDistribution);
        Assert.Equal(originalStats.FirstPlayed, roundTrippedStats.FirstPlayed);
        Assert.Equal(originalStats.LastPlayed, roundTrippedStats.LastPlayed);
    }

    [Fact]
    public void Entity_PartitionKey_AlwaysGameStats()
    {
        // Arrange
        var gameStats = new GameStats { Initials = "PKT" };

        // Act
        var entity = GameStatsEntity.FromGameStats(gameStats);

        // Assert
        Assert.Equal("GameStats", entity.PartitionKey);
    }

    [Fact]
    public void Entity_RowKey_MatchesInitials()
    {
        // Arrange
        var initials = "RKT";
        var gameStats = new GameStats { Initials = initials };

        // Act
        var entity = GameStatsEntity.FromGameStats(gameStats);

        // Assert
        Assert.Equal(initials, entity.RowKey);
    }

    [Fact]
    public void ToGameStats_WithNullDistribution_HandlesGracefully()
    {
        // Arrange
        var entity = new GameStatsEntity
        {
            PartitionKey = "GameStats",
            RowKey = "NUL",
            ScoreDistribution = string.Empty
        };

        // Act
        var gameStats = entity.ToGameStats();

        // Assert
        Assert.NotNull(gameStats);
    }

    [Fact]
    public void FromGameStats_WithZeroGames_CreatesValidEntity()
    {
        // Arrange
        var gameStats = new GameStats
        {
            Initials = "ZER",
            TotalGames = 0,
            HighestScore = 0,
            AverageScore = 0
        };

        // Act
        var entity = GameStatsEntity.FromGameStats(gameStats);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(0, entity.TotalGames);
        Assert.Equal(0, entity.HighestScore);
        Assert.Equal(0, entity.AverageScore);
    }
}

