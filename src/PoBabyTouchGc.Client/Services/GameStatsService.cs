using PoBabyTouchGc.Shared.Models;

namespace PoBabyTouchGc.Client.Services;

/// <summary>
/// Centralized service for game statistics API operations
/// Applies Service Layer pattern for stats management
/// </summary>
public class GameStatsService
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<GameStatsService> _logger;

    public GameStatsService(ApiClient apiClient, ILogger<GameStatsService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    /// <summary>
    /// Get statistics for a specific player
    /// </summary>
    public async Task<GameStats?> GetPlayerStatsAsync(string initials)
    {
        var response = await _apiClient.GetAsync<GameStats>($"/api/stats/{initials}");

        if (response.Success && response.Data != null)
        {
            return response.Data;
        }

        return null;
    }

    /// <summary>
    /// Get statistics for all players
    /// </summary>
    public async Task<List<GameStats>> GetAllStatsAsync()
    {
        var response = await _apiClient.GetAsync<IEnumerable<GameStats>>("/api/stats");

        if (response.Success && response.Data != null)
        {
            return response.Data.ToList();
        }

        return new List<GameStats>();
    }

    /// <summary>
    /// Record a game session and update statistics
    /// </summary>
    public async Task<GameStats?> RecordGameSessionAsync(
        string initials, 
        int score, 
        int circlesTapped, 
        int playtimeSeconds)
    {
        var request = new
        {
            Initials = initials,
            Score = score,
            CirclesTapped = circlesTapped,
            PlaytimeSeconds = playtimeSeconds
        };

        var response = await _apiClient.PostAsync<object, GameStats>("/api/stats/record", request);

        if (response.Success && response.Data != null)
        {
            return response.Data;
        }

        _logger.LogError("Failed to record game session: {Message}", response.Message);
        return null;
    }

    /// <summary>
    /// Parse score distribution string into chart data
    /// </summary>
    public List<ScoreDistributionData> ParseScoreDistribution(string? distribution)
    {
        var data = new List<ScoreDistributionData>();

        if (string.IsNullOrEmpty(distribution))
            return data;

        foreach (var entry in distribution.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = entry.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out int count))
            {
                data.Add(new ScoreDistributionData
                {
                    Range = parts[0],
                    Count = count
                });
            }
        }

        return data;
    }
}

/// <summary>
/// Data model for score distribution chart
/// </summary>
public class ScoreDistributionData
{
    public string Range { get; set; } = string.Empty;
    public int Count { get; set; }
}
