using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PoBabyTouchGc.Api.Features.Statistics;
using System.Net.Http.Json;
using Xunit;
using Azure.Data.Tables;
using PoBabyTouchGc.Api.Models;

namespace PoBabyTouchGc.Tests.Integration;

/// <summary>
/// Integration tests for game statistics functionality
/// Tests the complete flow from API to Azure Table Storage (Azurite via Testcontainers)
/// Addresses Priority 5: Test Coverage Gaps
/// </summary>
[Collection(AzuriteCollection.Name)]
public class GameStatsIntegrationTests : IAsyncLifetime
{
    private readonly AzuriteWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly List<string> _testInitials = new();

    public GameStatsIntegrationTests(AzuriteWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await CleanupTestData();
    }

    private async Task CleanupTestData()
    {
        if (_testInitials.Count == 0) return;

        try
        {
            using var scope = _factory.Services.CreateScope();
            var tableServiceClient = scope.ServiceProvider.GetRequiredService<TableServiceClient>();
            var tableClient = tableServiceClient.GetTableClient("PoBabyTouchGcGameStats");

            foreach (var initials in _testInitials)
            {
                await foreach (var entity in tableClient.QueryAsync<TableEntity>(
                    e => e.PartitionKey == "GameStats" && e.RowKey == initials))
                {
                    await tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cleanup failed: {ex.Message}");
        }
    }

    private void TrackInitials(string initials)
    {
        if (!_testInitials.Contains(initials))
        {
            _testInitials.Add(initials);
        }
    }

    [Fact]
    public async Task RecordGameSession_ValidRequest_UpdatesAllStats()
    {
        // Arrange
        var initials = "TST" + Guid.NewGuid().ToString("N")[..3].ToUpper();
        TrackInitials(initials);

        var request = new
        {
            Initials = initials,
            Score = 1500,
            CirclesTapped = 75,
            PlaytimeSeconds = 180
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/stats/record", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<GameStats>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(initials, apiResponse.Data.Initials);
        Assert.Equal(1, apiResponse.Data.TotalGames);
        Assert.Equal(1500, apiResponse.Data.HighestScore);
        Assert.Equal(1500, apiResponse.Data.AverageScore);
        Assert.Equal(75, apiResponse.Data.TotalCirclesTapped);
        Assert.Equal(180, apiResponse.Data.TotalPlaytimeSeconds);
    }

    [Fact]
    public async Task RecordGameSession_MultipleGames_CalculatesAverageCorrectly()
    {
        // Arrange
        var initials = "AVG" + Guid.NewGuid().ToString("N")[..3].ToUpper();
        TrackInitials(initials);

        // Record three games
        await _client.PostAsJsonAsync("/api/stats/record", new
        {
            Initials = initials,
            Score = 1000,
            CirclesTapped = 50,
            PlaytimeSeconds = 60
        });

        await _client.PostAsJsonAsync("/api/stats/record", new
        {
            Initials = initials,
            Score = 1500,
            CirclesTapped = 75,
            PlaytimeSeconds = 90
        });

        // Act
        var response = await _client.PostAsJsonAsync("/api/stats/record", new
        {
            Initials = initials,
            Score = 2000,
            CirclesTapped = 100,
            PlaytimeSeconds = 120
        });

        // Assert
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<GameStats>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(3, apiResponse.Data.TotalGames);
        Assert.Equal(2000, apiResponse.Data.HighestScore);
        Assert.Equal(1500, apiResponse.Data.AverageScore); // (1000 + 1500 + 2000) / 3
        Assert.Equal(225, apiResponse.Data.TotalCirclesTapped);
        Assert.Equal(270, apiResponse.Data.TotalPlaytimeSeconds);
    }

    [Fact]
    public async Task GetPlayerStats_ExistingPlayer_ReturnsCompleteData()
    {
        // Arrange
        var initials = "GET" + Guid.NewGuid().ToString("N")[..3].ToUpper();
        TrackInitials(initials);

        await _client.PostAsJsonAsync("/api/stats/record", new
        {
            Initials = initials,
            Score = 1200,
            CirclesTapped = 60,
            PlaytimeSeconds = 90
        });

        // Act
        var response = await _client.GetAsync($"/api/stats/{initials}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<GameStats>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(initials, apiResponse.Data.Initials);
        Assert.Equal(1200, apiResponse.Data.HighestScore);
    }

    [Fact]
    public async Task GetPlayerStats_NonExistentPlayer_ReturnsNotFound()
    {
        // Arrange
        var initials = "ZZZ" + Guid.NewGuid().ToString("N")[..3].ToUpper();

        // Act
        var response = await _client.GetAsync($"/api/stats/{initials}");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<GameStats>>();
        Assert.NotNull(apiResponse);
        Assert.False(apiResponse.Success);
        Assert.Null(apiResponse.Data);
    }

    [Fact]
    public async Task GetAllStats_MultiplePlayers_ReturnsAllData()
    {
        // Arrange
        var initials1 = "AL1" + Guid.NewGuid().ToString("N")[..3].ToUpper();
        var initials2 = "AL2" + Guid.NewGuid().ToString("N")[..3].ToUpper();
        TrackInitials(initials1);
        TrackInitials(initials2);

        await _client.PostAsJsonAsync("/api/stats/record", new
        {
            Initials = initials1,
            Score = 1000,
            CirclesTapped = 50,
            PlaytimeSeconds = 60
        });

        await _client.PostAsJsonAsync("/api/stats/record", new
        {
            Initials = initials2,
            Score = 1500,
            CirclesTapped = 75,
            PlaytimeSeconds = 90
        });

        // Act
        var response = await _client.GetAsync("/api/stats");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<GameStats>>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        
        var statsList = apiResponse.Data.ToList();
        Assert.Contains(statsList, s => s.Initials == initials1);
        Assert.Contains(statsList, s => s.Initials == initials2);
    }

    [Fact]
    public async Task UpdatePercentileRank_MultipleScores_CalculatesCorrectly()
    {
        // Arrange - Create 5 players with different scores
        // Using very high scores to avoid collision with other tests
        var players = new[]
        {
            ("PR1" + Guid.NewGuid().ToString("N")[..3].ToUpper(), 900000),
            ("PR2" + Guid.NewGuid().ToString("N")[..3].ToUpper(), 910000),
            ("PR3" + Guid.NewGuid().ToString("N")[..3].ToUpper(), 920000),
            ("PR4" + Guid.NewGuid().ToString("N")[..3].ToUpper(), 930000),
            ("PR5" + Guid.NewGuid().ToString("N")[..3].ToUpper(), 940000)
        };

        foreach (var (initials, score) in players)
        {
            TrackInitials(initials);
            await _client.PostAsJsonAsync("/api/stats/record", new
            {
                Initials = initials,
                Score = score,
                CirclesTapped = 50,
                PlaytimeSeconds = 60
            });
        }

        // Small delay to ensure all records are persisted
        await Task.Delay(100);

        // Act - Get each player's stats and verify percentile is calculated
        for (int i = 0; i < players.Length; i++)
        {
            var response = await _client.GetAsync($"/api/stats/{players[i].Item1}");
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<GameStats>>();
            
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
            
            // Verify percentile is calculated (between 0-100)
            Assert.InRange(apiResponse.Data.PercentileRank, 0.0, 100.0);
        }
        
        // Verify the first player has the lowest percentile among the 5
        var firstPlayerResponse = await _client.GetAsync($"/api/stats/{players[0].Item1}");
        var firstPlayerData = (await firstPlayerResponse.Content.ReadFromJsonAsync<ApiResponse<GameStats>>())!.Data!;
        
        var lastPlayerResponse = await _client.GetAsync($"/api/stats/{players[4].Item1}");
        var lastPlayerData = (await lastPlayerResponse.Content.ReadFromJsonAsync<ApiResponse<GameStats>>())!.Data!;
        
        // Last player (highest score) should have higher or equal percentile than first
        Assert.True(lastPlayerData.PercentileRank >= firstPlayerData.PercentileRank, 
            $"Expected highest scorer ({lastPlayerData.HighestScore}) to have >= percentile than lowest ({firstPlayerData.HighestScore})");
    }

    [Fact]
    public async Task ScoreDistribution_MultipleBuckets_ParsesCorrectly()
    {
        // Arrange
        var initials = "DIS" + Guid.NewGuid().ToString("N")[..3].ToUpper();
        TrackInitials(initials);

        // Record games with scores in different buckets
        await _client.PostAsJsonAsync("/api/stats/record", new
        {
            Initials = initials,
            Score = 50,
            CirclesTapped = 25,
            PlaytimeSeconds = 30
        });

        await _client.PostAsJsonAsync("/api/stats/record", new
        {
            Initials = initials,
            Score = 150,
            CirclesTapped = 30,
            PlaytimeSeconds = 40
        });

        await _client.PostAsJsonAsync("/api/stats/record", new
        {
            Initials = initials,
            Score = 250,
            CirclesTapped = 35,
            PlaytimeSeconds = 50
        });

        // Act
        var response = await _client.GetAsync($"/api/stats/{initials}");

        // Assert
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<GameStats>>();
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.NotNull(apiResponse.Data.ScoreDistribution);
        
        // Should have 3 different buckets
        Assert.Contains("0-99:1", apiResponse.Data.ScoreDistribution);
        Assert.Contains("100-199:1", apiResponse.Data.ScoreDistribution);
        Assert.Contains("200-299:1", apiResponse.Data.ScoreDistribution);
    }

    [Fact]
    public async Task GameStatsService_CompleteFlow_WorksEndToEnd()
    {
        // Arrange
        var initials = "E2E" + Guid.NewGuid().ToString("N")[..3].ToUpper();
        TrackInitials(initials);

        using var scope = _factory.Services.CreateScope();
        var gameStatsRepository = scope.ServiceProvider.GetRequiredService<IGameStatsRepository>();

        // Act & Assert - Record a game session
        var stats = await gameStatsRepository.RecordGameSessionAsync(initials, 1800, 90, 120);
        Assert.NotNull(stats);
        Assert.Equal(initials, stats.Initials);
        Assert.Equal(1800, stats.HighestScore);

        // Act & Assert - Get stats
        var retrievedStats = await gameStatsRepository.GetStatsAsync(initials);
        Assert.NotNull(retrievedStats);
        Assert.Equal(1800, retrievedStats.HighestScore);

        // Act & Assert - Record another session
        var updatedStats = await gameStatsRepository.RecordGameSessionAsync(initials, 2000, 100, 130);
        Assert.NotNull(updatedStats);
        Assert.Equal(2, updatedStats.TotalGames);
        Assert.Equal(2000, updatedStats.HighestScore);
        Assert.Equal(1900, updatedStats.AverageScore); // (1800 + 2000) / 2
    }
}

