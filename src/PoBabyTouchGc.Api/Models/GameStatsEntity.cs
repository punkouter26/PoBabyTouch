using Azure;
using Azure.Data.Tables;

namespace PoBabyTouchGc.Api.Models;

/// <summary>
/// Azure Table Storage entity for GameStats
/// Applies Strategy Pattern for data persistence
/// </summary>
public class GameStatsEntity : ITableEntity
{
    // ITableEntity required properties
    public string PartitionKey { get; set; } = "GameStats";
    public string RowKey { get; set; } = string.Empty; // Initials
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    // GameStats properties
    public string Initials { get; set; } = string.Empty;
    public int TotalGames { get; set; }
    public int TotalCirclesTapped { get; set; }
    public double AverageScore { get; set; }
    public int HighestScore { get; set; }
    public int LongestStreak { get; set; }
    public int TotalPlaytimeSeconds { get; set; }
    public DateTime LastPlayed { get; set; }
    public DateTime FirstPlayed { get; set; }
    public double PercentileRank { get; set; }
    public string ScoreDistribution { get; set; } = string.Empty;

    public GameStatsEntity() { }

    public GameStatsEntity(string initials)
    {
        PartitionKey = "GameStats";
        RowKey = initials.ToUpperInvariant();
        Initials = initials.ToUpperInvariant();
        FirstPlayed = DateTime.UtcNow;
        LastPlayed = DateTime.UtcNow;
    }

    /// <summary>
    /// Convert to domain model
    /// </summary>
    public GameStats ToGameStats()
    {
        return new GameStats
        {
            Initials = Initials,
            TotalGames = TotalGames,
            TotalCirclesTapped = TotalCirclesTapped,
            AverageScore = AverageScore,
            HighestScore = HighestScore,
            LongestStreak = LongestStreak,
            TotalPlaytimeSeconds = TotalPlaytimeSeconds,
            LastPlayed = LastPlayed,
            FirstPlayed = FirstPlayed,
            PercentileRank = PercentileRank,
            ScoreDistribution = ScoreDistribution
        };
    }

    /// <summary>
    /// Create from domain model
    /// </summary>
    public static GameStatsEntity FromGameStats(GameStats stats)
    {
        return new GameStatsEntity
        {
            PartitionKey = "GameStats",
            RowKey = stats.Initials.ToUpperInvariant(),
            Initials = stats.Initials,
            TotalGames = stats.TotalGames,
            TotalCirclesTapped = stats.TotalCirclesTapped,
            AverageScore = stats.AverageScore,
            HighestScore = stats.HighestScore,
            LongestStreak = stats.LongestStreak,
            TotalPlaytimeSeconds = stats.TotalPlaytimeSeconds,
            LastPlayed = stats.LastPlayed,
            FirstPlayed = stats.FirstPlayed,
            PercentileRank = stats.PercentileRank,
            ScoreDistribution = stats.ScoreDistribution
        };
    }
}
