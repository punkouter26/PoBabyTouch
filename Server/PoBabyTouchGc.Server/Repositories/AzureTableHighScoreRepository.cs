using PoBabyTouchGc.Shared.Models;
using Azure.Data.Tables;

namespace PoBabyTouchGc.Server.Repositories
{
    /// <summary>
    /// Azure Table Storage implementation of the high score repository
    /// Applying Repository Pattern to encapsulate data access logic
    /// Follows SOLID principles - Single Responsibility and Open/Closed
    /// </summary>
    public class AzureTableHighScoreRepository : IHighScoreRepository
    {
        private readonly ILogger<AzureTableHighScoreRepository> _logger;
        private const string TableName = "PoBabyTouchGcHighScores";
        private readonly TableClient _tableClient;

        public AzureTableHighScoreRepository(TableServiceClient tableServiceClient, ILogger<AzureTableHighScoreRepository> logger)
        {
            _logger = logger;
            _tableClient = tableServiceClient.GetTableClient(TableName);
            _tableClient.CreateIfNotExists(); // Ensure the table is created
        }

        public async Task<bool> SaveHighScoreAsync(HighScore highScore)
        {
            try
            {
                // Ensure proper initialization
                if (string.IsNullOrEmpty(highScore.PartitionKey))
                {
                    highScore.PartitionKey = highScore.GameMode;
                }

                if (string.IsNullOrEmpty(highScore.RowKey))
                {
                    // Use the constructor's logic for RowKey generation
                    highScore.RowKey = $"{(999999 - highScore.Score):D6}_{highScore.ScoreDate:yyyyMMddHHmmss}_{Guid.NewGuid():N}";
                }

                highScore.Timestamp = DateTimeOffset.UtcNow;

                await _tableClient.AddEntityAsync(highScore);

                _logger.LogDebug("High score saved to Azure Table Storage: {PlayerInitials} - {Score}",
                    highScore.PlayerInitials, highScore.Score);
                return true;
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 409)
            {
                // Handle duplicate entity conflict by updating the RowKey with more uniqueness
                highScore.RowKey = $"{(999999 - highScore.Score):D6}_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}";
                try
                {
                    await _tableClient.AddEntityAsync(highScore);
                    _logger.LogDebug("High score saved after retry: {PlayerInitials} - {Score}",
                        highScore.PlayerInitials, highScore.Score);
                    return true;
                }
                catch (Exception retryEx)
                {
                    _logger.LogError(retryEx, "Failed to save high score after retry");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save high score to Azure Table Storage");
                return false;
            }
        }

        public async Task<List<HighScore>> GetTopScoresAsync(int count, string gameMode)
        {
            try
            {

                var query = _tableClient.QueryAsync<HighScore>(
                    entity => entity.PartitionKey == gameMode,
                    maxPerPage: count * 2 // Get extra to sort properly
                );

                var highScores = new List<HighScore>();
                await foreach (var score in query)
                {
                    highScores.Add(score);
                    if (highScores.Count >= count * 2) break; // Limit to prevent excessive data
                }

                // Sort by score descending and take top count
                var sortedScores = highScores.OrderByDescending(s => s.Score).Take(count).ToList();

                _logger.LogDebug("Retrieved {Count} top scores for {GameMode}", sortedScores.Count, gameMode);
                return sortedScores;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get top scores from Azure Table Storage");
                return new List<HighScore>();
            }
        }

        public async Task<bool> IsHighScoreAsync(int score, string gameMode)
        {
            try
            {

                var query = _tableClient.QueryAsync<HighScore>(
                    entity => entity.PartitionKey == gameMode,
                    maxPerPage: 10 // Just check top 10
                );

                var highScores = new List<HighScore>();
                await foreach (var highScore in query)
                {
                    highScores.Add(highScore);
                }

                // Sort by score descending and check if the provided score would be in top 10
                var sortedScores = highScores.OrderByDescending(s => s.Score).ToList();

                if (sortedScores.Count < 10)
                {
                    return true; // Less than 10 scores, so this one qualifies
                }

                return score > sortedScores[9].Score; // Check if score is better than 10th place
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if score is high score");
                return false;
            }
        }

        public async Task<int> GetPlayerRankAsync(int score, string gameMode)
        {
            try
            {

                var query = _tableClient.QueryAsync<HighScore>(
                    entity => entity.PartitionKey == gameMode
                );

                var highScores = new List<HighScore>();
                await foreach (var highScore in query)
                {
                    highScores.Add(highScore);
                }

                var sortedScores = highScores.OrderByDescending(s => s.Score).ToList();

                // Find the rank (1-based)
                int rank = 1;
                foreach (var highScore in sortedScores)
                {
                    if (score >= highScore.Score)
                    {
                        return rank;
                    }
                    rank++;
                }

                return rank; // If not found, return the next rank
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get player rank");
                return -1;
            }
        }

        public async Task<bool> DeleteHighScoreAsync(string gameMode, string rowKey)
        {
            try
            {
                await _tableClient.DeleteEntityAsync(gameMode, rowKey);

                _logger.LogDebug("High score deleted from Azure Table Storage: {GameMode}/{RowKey}", gameMode, rowKey);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete high score from Azure Table Storage");
                return false;
            }
        }

        public async Task<int> GetTotalScoresCountAsync(string gameMode)
        {
            try
            {

                var query = _tableClient.QueryAsync<HighScore>(
                    entity => entity.PartitionKey == gameMode
                );

                int count = 0;
                await foreach (var _ in query)
                {
                    count++;
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get total scores count");
                return 0;
            }
        }
    }
}
