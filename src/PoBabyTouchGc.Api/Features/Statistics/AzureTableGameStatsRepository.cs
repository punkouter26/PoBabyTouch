using Azure.Data.Tables;
using PoBabyTouchGc.Api.Models;

namespace PoBabyTouchGc.Api.Features.Statistics;

/// <summary>
/// Azure Table Storage implementation of <see cref="IGameStatsRepository"/>.
/// Tracks cumulative player statistics across all game sessions.
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
        _tableClient.CreateIfNotExists();
    }

    public async Task<GameStats?> GetStatsAsync(string initials)
    {
        try
        {
            var normalizedInitials = initials.ToUpperInvariant();
            var response = await _tableClient.GetEntityAsync<GameStatsEntity>(
                "GameStats", normalizedInitials);
            return response.Value?.ToGameStats();
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogInformation("Stats not found for {Initials}", initials);
            return null;
        }
    }

    public async Task<GameStats> UpsertStatsAsync(GameStats stats)
    {
        var entity = GameStatsEntity.FromGameStats(stats);
        await _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);
        _logger.LogInformation("Stats upserted for {Initials}", stats.Initials);
        return stats;
    }

    public async Task<IEnumerable<GameStats>> GetAllStatsAsync()
    {
        var stats = new List<GameStats>();
        await foreach (var entity in _tableClient.QueryAsync<GameStatsEntity>(
            filter: e => e.PartitionKey == "GameStats"))
        {
            stats.Add(entity.ToGameStats());
        }
        return stats;
    }

    public async Task<GameStats> RecordGameSessionAsync(
        string initials, int score, int circlesTapped, int playtimeSeconds)
    {
        var normalizedInitials = initials.ToUpperInvariant();
        var stats = await GetStatsAsync(normalizedInitials) ?? new GameStats
        {
            Initials = normalizedInitials,
            FirstPlayed = DateTime.UtcNow
        };

        stats.TotalGames++;
        stats.TotalCirclesTapped += circlesTapped;
        stats.TotalPlaytimeSeconds += playtimeSeconds;
        stats.LastPlayed = DateTime.UtcNow;

        if (score > stats.HighestScore)
            stats.HighestScore = score;

        stats.AverageScore = ((stats.AverageScore * (stats.TotalGames - 1)) + score) / stats.TotalGames;
        stats.ScoreDistribution = UpdateScoreDistribution(stats.ScoreDistribution, score);

        await UpdatePercentileRankAsync(stats);
        await UpsertStatsAsync(stats);

        _logger.LogInformation("Game session recorded for {Initials}: Score={Score}, TotalGames={TotalGames}",
            normalizedInitials, score, stats.TotalGames);
        return stats;
    }

    private string UpdateScoreDistribution(string currentDistribution, int score)
    {
        var buckets = new Dictionary<string, int>();

        if (!string.IsNullOrEmpty(currentDistribution))
        {
            foreach (var entry in currentDistribution.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = entry.Split(':');
                if (parts.Length == 2) buckets[parts[0]] = int.Parse(parts[1]);
            }
        }

        var bucket = GetScoreBucket(score);
        buckets[bucket] = buckets.GetValueOrDefault(bucket) + 1;

        return string.Join(',', buckets.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
    }

    private static string GetScoreBucket(int score)
    {
        var lower = (score / 100) * 100;
        return $"{lower}-{lower + 99}";
    }

    private async Task UpdatePercentileRankAsync(GameStats currentStats)
    {
        var allStats = (await GetAllStatsAsync()).ToList();
        if (allStats.Count <= 1)
        {
            currentStats.PercentileRank = 100.0;
            return;
        }

        var playersWithLowerScore = allStats.Count(s => s.HighestScore < currentStats.HighestScore);
        currentStats.PercentileRank = Math.Round((double)playersWithLowerScore / allStats.Count * 100, 1);
    }
}
