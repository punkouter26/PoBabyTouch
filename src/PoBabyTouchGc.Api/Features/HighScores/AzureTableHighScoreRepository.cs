using Azure.Data.Tables;
using PoBabyTouchGc.Api.Models;

namespace PoBabyTouchGc.Api.Features.HighScores;

/// <summary>
/// Azure Table Storage implementation of <see cref="IHighScoreRepository"/>.
/// Uses composite RowKey for natural sort-order (Repository Pattern).
/// </summary>
public class AzureTableHighScoreRepository : IHighScoreRepository
{
    private readonly ILogger<AzureTableHighScoreRepository> _logger;
    private readonly TableClient _tableClient;
    private const string TableName = "PoBabyTouchHighScores";

    public AzureTableHighScoreRepository(
        TableServiceClient tableServiceClient,
        ILogger<AzureTableHighScoreRepository> logger)
    {
        _logger = logger;
        _tableClient = tableServiceClient.GetTableClient(TableName);
        _tableClient.CreateIfNotExists();
    }

    public async Task<bool> SaveHighScoreAsync(HighScore highScore)
    {
        try
        {
            if (string.IsNullOrEmpty(highScore.PartitionKey))
                highScore.PartitionKey = highScore.GameMode;

            if (string.IsNullOrEmpty(highScore.RowKey))
                highScore.RowKey = $"{(999999 - highScore.Score):D6}_{highScore.ScoreDate:yyyyMMddHHmmss}_{Guid.NewGuid():N}";

            highScore.Timestamp = DateTimeOffset.UtcNow;
            await _tableClient.AddEntityAsync(highScore);

            _logger.LogDebug("High score saved: {PlayerInitials} - {Score}",
                highScore.PlayerInitials, highScore.Score);
            return true;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 409)
        {
            // Retry with a new RowKey on conflict
            highScore.RowKey = $"{(999999 - highScore.Score):D6}_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}";
            try
            {
                await _tableClient.AddEntityAsync(highScore);
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
            _logger.LogError(ex, "Failed to save high score");
            return false;
        }
    }

    public async Task<List<HighScore>> GetTopScoresAsync(int count, string gameMode)
    {
        try
        {
            var query = _tableClient.QueryAsync<HighScore>(
                entity => entity.PartitionKey == gameMode,
                maxPerPage: count * 2);

            var highScores = new List<HighScore>();
            await foreach (var score in query)
            {
                highScores.Add(score);
                if (highScores.Count >= count * 2) break;
            }

            return highScores.OrderByDescending(s => s.Score).Take(count).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get top scores");
            return [];
        }
    }

    public async Task<bool> IsHighScoreAsync(int score, string gameMode)
    {
        try
        {
            var highScores = new List<HighScore>();
            await foreach (var hs in _tableClient.QueryAsync<HighScore>(
                entity => entity.PartitionKey == gameMode, maxPerPage: 10))
            {
                highScores.Add(hs);
            }

            var sorted = highScores.OrderByDescending(s => s.Score).ToList();
            return sorted.Count < 10 || score > sorted[9].Score;
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
            var highScores = new List<HighScore>();
            await foreach (var hs in _tableClient.QueryAsync<HighScore>(
                entity => entity.PartitionKey == gameMode))
            {
                highScores.Add(hs);
            }

            int rank = 1;
            foreach (var hs in highScores.OrderByDescending(s => s.Score))
            {
                if (score >= hs.Score) return rank;
                rank++;
            }

            return rank;
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
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete high score");
            return false;
        }
    }

    public async Task<int> GetTotalScoresCountAsync(string gameMode)
    {
        try
        {
            int count = 0;
            await foreach (var _ in _tableClient.QueryAsync<HighScore>(
                entity => entity.PartitionKey == gameMode))
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
