using Azure;
using Azure.Data.Tables;
using System;

namespace PoBabyTouchGc.Shared.Models;

/// <summary>
/// Azure Table Storage entity for high scores
/// </summary>
public class HighScore : ITableEntity
{
    /// <summary>
    /// Player's 3-letter initials
    /// </summary>
    public string PlayerInitials { get; set; } = string.Empty;

    /// <summary>
    /// Player's score
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Game mode (e.g., "Easy", "Normal", "Hard")
    /// </summary>
    public string GameMode { get; set; } = "Default";

    /// <summary>
    /// Date and time when the score was achieved
    /// </summary>
    public DateTime ScoreDate { get; set; } = DateTime.UtcNow;

    // ITableEntity implementation
    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public HighScore()
    {
    }

    public HighScore(string gameMode, string playerInitials, int score)
    {
        GameMode = gameMode;
        PlayerInitials = playerInitials.ToUpper();
        Score = score;
        ScoreDate = DateTime.UtcNow;

        // Partition by game mode for better performance
        PartitionKey = gameMode;

        // Row key combines negative score (for sorting) and timestamp for uniqueness
        // Negative score ensures higher scores appear first when sorted
        RowKey = $"{(999999 - score):D6}_{ScoreDate:yyyyMMddHHmmss}_{Guid.NewGuid():N}";
    }
}
