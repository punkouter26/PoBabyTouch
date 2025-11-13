using Azure.Data.Tables;
using PoBabyTouchGc.Shared.Models;

namespace PoBabyTouchGc.Api.Features.Statistics;

/// <summary>
/// Azure Table Storage implementation of game statistics repository
/// Applying Repository Pattern and Strategy Pattern
/// </summary>
public class AzureTableGameStatsRepository : IGameStatsRepository
{
    private readonly TableClient _tableClient;
    private readonly ILogger<AzureTableGameStatsRepository> _logger;
    private const string TableName = "PoBabyTouchGcGameStats";

    public AzureTableGameStatsRepository(
        TableServiceClient tableServiceClient,
        ILogger<AzureTableGameStatsRepository> logger)
    {
        _tableClient = tableServiceClient.GetTableClient(TableName);
        _logger = logger;
        
        // Ensure table exists
        _tableClient.CreateIfNotExists();
        _logger.LogInformation("GameStats table initialized: {TableName}", TableName);
    }

    public async Task<GameStats?> GetStatsAsync(string initials)
    {
        try
        {
            var normalizedInitials = initials.ToUpperInvariant();
            var response = await _tableClient.GetEntityAsync<GameStatsEntity>(
                "GameStats", 
                normalizedInitials);
            
            return response.Value?.ToGameStats();
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogInformation("Stats not found for initials: {Initials}", initials);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stats for {Initials}", initials);
            throw;
        }
    }

    public async Task<GameStats> UpsertStatsAsync(GameStats stats)
    {
        try
        {
            var entity = GameStatsEntity.FromGameStats(stats);
            await _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);
            
            _logger.LogInformation("Stats upserted for {Initials}", stats.Initials);
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting stats for {Initials}", stats.Initials);
            throw;
        }
    }

    public async Task<IEnumerable<GameStats>> GetAllStatsAsync()
    {
        try
        {
            var stats = new List<GameStats>();
            
            await foreach (var entity in _tableClient.QueryAsync<GameStatsEntity>(
                filter: e => e.PartitionKey == "GameStats"))
            {
                stats.Add(entity.ToGameStats());
            }
            
            _logger.LogInformation("Retrieved {Count} player stats", stats.Count);
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all stats");
            throw;
        }
    }

    public async Task<GameStats> RecordGameSessionAsync(
        string initials, 
        int score, 
        int circlesTapped, 
        int playtimeSeconds)
    {
        try
        {
            var normalizedInitials = initials.ToUpperInvariant();
            
            // Get existing stats or create new
            var stats = await GetStatsAsync(normalizedInitials) ?? new GameStats
            {
                Initials = normalizedInitials,
                FirstPlayed = DateTime.UtcNow
            };
            
            // Update statistics
            stats.TotalGames++;
            stats.TotalCirclesTapped += circlesTapped;
            stats.TotalPlaytimeSeconds += playtimeSeconds;
            stats.LastPlayed = DateTime.UtcNow;
            
            // Update highest score
            if (score > stats.HighestScore)
            {
                stats.HighestScore = score;
            }
            
            // Calculate average score
            stats.AverageScore = ((stats.AverageScore * (stats.TotalGames - 1)) + score) / stats.TotalGames;
            
            // Update score distribution
            stats.ScoreDistribution = UpdateScoreDistribution(stats.ScoreDistribution, score);
            
            // Calculate percentile rank
            await UpdatePercentileRankAsync(stats);
            
            // Save to storage
            await UpsertStatsAsync(stats);
            
            _logger.LogInformation(
                "Game session recorded for {Initials}: Score={Score}, TotalGames={TotalGames}",
                normalizedInitials, score, stats.TotalGames);
            
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording game session for {Initials}", initials);
            throw;
        }
    }

    /// <summary>
    /// Update score distribution string with new score
    /// </summary>
    private string UpdateScoreDistribution(string currentDistribution, int score)
    {
        // Parse existing distribution
        var buckets = new Dictionary<string, int>();
        
        if (!string.IsNullOrEmpty(currentDistribution))
        {
            foreach (var entry in currentDistribution.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = entry.Split(':');
                if (parts.Length == 2)
                {
                    buckets[parts[0]] = int.Parse(parts[1]);
                }
            }
        }
        
        // Determine bucket for new score
        var bucket = GetScoreBucket(score);
        
        // Increment bucket count
        if (buckets.ContainsKey(bucket))
        {
            buckets[bucket]++;
        }
        else
        {
            buckets[bucket] = 1;
        }
        
        // Convert back to string
        return string.Join(',', buckets.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
    }

    /// <summary>
    /// Get score bucket (0-100, 101-200, etc.)
    /// </summary>
    private string GetScoreBucket(int score)
    {
        var lowerBound = (score / 100) * 100;
        var upperBound = lowerBound + 99;
        return $"{lowerBound}-{upperBound}";
    }

    /// <summary>
    /// Calculate and update percentile rank
    /// </summary>
    private async Task UpdatePercentileRankAsync(GameStats currentStats)
    {
        var allStats = await GetAllStatsAsync();
        var totalPlayers = allStats.Count();
        
        if (totalPlayers <= 1)
        {
            currentStats.PercentileRank = 100.0;
            return;
        }
        
        var playersWithLowerScore = allStats.Count(s => s.HighestScore < currentStats.HighestScore);
        currentStats.PercentileRank = Math.Round((double)playersWithLowerScore / totalPlayers * 100, 1);
    }
}
