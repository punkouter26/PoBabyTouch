namespace PoBabyTouchGc.Shared.Models;

/// <summary>
/// Represents aggregated game statistics for a player
/// Stored in Azure Table Storage with PartitionKey = "GameStats" and RowKey = Initials
/// </summary>
public class GameStats
{
    /// <summary>
    /// Player initials (3 characters)
    /// </summary>
    public string Initials { get; set; } = string.Empty;
    
    /// <summary>
    /// Total number of games played
    /// </summary>
    public int TotalGames { get; set; }
    
    /// <summary>
    /// Total circles tapped across all games
    /// </summary>
    public int TotalCirclesTapped { get; set; }
    
    /// <summary>
    /// Average score across all games
    /// </summary>
    public double AverageScore { get; set; }
    
    /// <summary>
    /// Highest score ever achieved
    /// </summary>
    public int HighestScore { get; set; }
    
    /// <summary>
    /// Longest streak of games played
    /// </summary>
    public int LongestStreak { get; set; }
    
    /// <summary>
    /// Total playtime in seconds
    /// </summary>
    public int TotalPlaytimeSeconds { get; set; }
    
    /// <summary>
    /// Last time the player played (UTC)
    /// </summary>
    public DateTime LastPlayed { get; set; }
    
    /// <summary>
    /// First time the player played (UTC)
    /// </summary>
    public DateTime FirstPlayed { get; set; }
    
    /// <summary>
    /// Percentile rank compared to all players (0-100)
    /// </summary>
    public double PercentileRank { get; set; }
    
    /// <summary>
    /// Distribution of scores (for histogram)
    /// Format: "0-100:5,101-200:10,201-300:15" (score range:count)
    /// </summary>
    public string ScoreDistribution { get; set; } = string.Empty;
}
