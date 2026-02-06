using PoBabyTouchGc.Api.Models;

namespace PoBabyTouchGc.Api.Features.HighScores;

/// <summary>
/// Repository interface for high score persistence (Repository Pattern).
/// </summary>
public interface IHighScoreRepository
{
    Task<bool> SaveHighScoreAsync(HighScore highScore);
    Task<List<HighScore>> GetTopScoresAsync(int count, string gameMode);
    Task<bool> IsHighScoreAsync(int score, string gameMode);
    Task<int> GetPlayerRankAsync(int score, string gameMode);
    Task<bool> DeleteHighScoreAsync(string gameMode, string rowKey);
    Task<int> GetTotalScoresCountAsync(string gameMode);
}
