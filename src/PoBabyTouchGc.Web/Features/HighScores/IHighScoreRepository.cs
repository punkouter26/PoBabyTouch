using PoBabyTouchGc.Shared.Models;

namespace PoBabyTouchGc.Web.Features.HighScores;

/// <summary>
/// Repository interface for high score data access
/// Applying Repository Pattern to abstract data access logic
/// Follows SOLID principles - Interface Segregation and Dependency Inversion
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
