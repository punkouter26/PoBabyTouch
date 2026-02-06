using PoBabyTouchGc.Api.Models;

namespace PoBabyTouchGc.Api.Features.Statistics;

/// <summary>
/// Repository interface for game statistics (Repository Pattern).
/// </summary>
public interface IGameStatsRepository
{
    Task<GameStats?> GetStatsAsync(string initials);
    Task<GameStats> UpsertStatsAsync(GameStats stats);
    Task<IEnumerable<GameStats>> GetAllStatsAsync();
    Task<GameStats> RecordGameSessionAsync(string initials, int score, int circlesTapped, int playtimeSeconds);
}
