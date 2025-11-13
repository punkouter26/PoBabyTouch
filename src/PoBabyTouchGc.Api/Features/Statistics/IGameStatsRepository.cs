using PoBabyTouchGc.Shared.Models;

namespace PoBabyTouchGc.Api.Features.Statistics;

/// <summary>
/// Repository interface for game statistics
/// Applying Repository Pattern
/// </summary>
public interface IGameStatsRepository
{
    /// <summary>
    /// Get statistics for a specific player
    /// </summary>
    Task<GameStats?> GetStatsAsync(string initials);
    
    /// <summary>
    /// Update or create statistics for a player
    /// </summary>
    Task<GameStats> UpsertStatsAsync(GameStats stats);
    
    /// <summary>
    /// Get all player statistics (for calculating percentiles)
    /// </summary>
    Task<IEnumerable<GameStats>> GetAllStatsAsync();
    
    /// <summary>
    /// Record a new game session and update stats
    /// </summary>
    Task<GameStats> RecordGameSessionAsync(string initials, int score, int circlesTapped, int playtimeSeconds);
}
