using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PoBabyTouchGc.Api.Features.HighScores;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using Azure.Data.Tables;
using PoBabyTouchGc.Api.Models;

namespace PoBabyTouchGc.Tests.Integration;

/// <summary>
/// Integration tests for high score functionality
/// Tests the complete flow from API to Azure Table Storage (Azurite via Testcontainers)
/// </summary>
[Collection(AzuriteCollection.Name)]
public class HighScoreIntegrationTests : IAsyncLifetime
{
    private readonly AzuriteWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly List<string> _testGameModes = new();

    public HighScoreIntegrationTests(AzuriteWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    /// <summary>
    /// Async cleanup of test data after each test
    /// </summary>
    public async Task DisposeAsync()
    {
        await CleanupTestData();
    }

        private async Task CleanupTestData()
        {
            if (_testGameModes.Count == 0) return;

            try
            {
                using var scope = _factory.Services.CreateScope();
                var tableServiceClient = scope.ServiceProvider.GetRequiredService<TableServiceClient>();
                var tableClient = tableServiceClient.GetTableClient("PoBabyTouchHighScores");

                // Delete all test data for the game modes used in this test
                foreach (var gameMode in _testGameModes)
                {
                    await foreach (var entity in tableClient.QueryAsync<TableEntity>(e => e.PartitionKey == gameMode))
                    {
                        await tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log cleanup failure but don't throw - tests should pass even if cleanup fails
                Console.WriteLine($"Cleanup failed: {ex.Message}");
            }
        }

        private void TrackGameMode(string gameMode)
        {
            if (!_testGameModes.Contains(gameMode))
            {
                _testGameModes.Add(gameMode);
            }
        }

        [Fact]
        public async Task SaveHighScore_ValidRequest_ReturnsOk()
        {
            // Arrange
            var gameMode = "TestSave_" + Guid.NewGuid().ToString("N")[..8];
            TrackGameMode(gameMode);

            var request = new
            {
                PlayerInitials = "ABC",
                Score = 1500,
                GameMode = gameMode
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/highscores", request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.Equal("High score saved successfully", apiResponse.Message);

            // Verify the score was actually saved
            var getResponse = await _client.GetAsync($"/api/highscores?gameMode={gameMode}");
            Assert.True(getResponse.IsSuccessStatusCode);
            var getApiResponse = await getResponse.Content.ReadFromJsonAsync<ApiResponse<List<HighScore>>>();
            Assert.NotNull(getApiResponse);
            Assert.True(getApiResponse.Success);
            Assert.NotNull(getApiResponse.Data);
            Assert.Single(getApiResponse.Data);
            Assert.Equal("ABC", getApiResponse.Data[0].PlayerInitials);
            Assert.Equal(1500, getApiResponse.Data[0].Score);
        }

        [Fact]
        public async Task SaveHighScore_InvalidInitials_ReturnsBadRequest()
        {
            // Arrange
            var gameMode = "TestInvalid_" + Guid.NewGuid().ToString("N")[..8];
            TrackGameMode(gameMode);

            var request = new
            {
                PlayerInitials = "AB", // Only 2 characters
                Score = 1500,
                GameMode = gameMode
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/highscores", request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.NotNull(apiResponse);
            Assert.False(apiResponse.Success);
            Assert.Contains("Player initials must be exactly 3 characters", apiResponse.Message);
        }

        [Fact]
        public async Task SaveHighScore_NegativeScore_ReturnsBadRequest()
        {
            // Arrange
            var gameMode = "TestNegative_" + Guid.NewGuid().ToString("N")[..8];
            TrackGameMode(gameMode);

            var request = new
            {
                PlayerInitials = "ABC",
                Score = -100,
                GameMode = gameMode
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/highscores", request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.NotNull(apiResponse);
            Assert.False(apiResponse.Success);
            Assert.Contains("Score cannot be negative", apiResponse.Message);
        }

        [Fact]
        public async Task GetTopScores_DefaultRequest_ReturnsScores()
        {
            // Arrange
            var gameMode = "TestGetTop_" + Guid.NewGuid().ToString("N")[..8];
            TrackGameMode(gameMode);

            var saveRequest = new
            {
                PlayerInitials = "XYZ",
                Score = 2000,
                GameMode = gameMode
            };
            await _client.PostAsJsonAsync("/api/highscores", saveRequest);

            // Act
            var response = await _client.GetAsync($"/api/highscores?gameMode={gameMode}");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<HighScore>>>();
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
            Assert.True(apiResponse.Data.Count > 0);

            // Verify our test score is there
            Assert.Equal("XYZ", apiResponse.Data[0].PlayerInitials);
            Assert.Equal(2000, apiResponse.Data[0].Score);
        }

        [Fact]
        public async Task GetTopScores_WithCount_ReturnsCorrectNumber()
        {
            // Arrange
            var gameMode = "TestCount_" + Guid.NewGuid().ToString("N")[..8];
            TrackGameMode(gameMode);

            var tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                var request = new
                {
                    PlayerInitials = $"T{i:D2}",
                    Score = 1000 + (i * 100),
                    GameMode = gameMode
                };
                tasks.Add(_client.PostAsJsonAsync("/api/highscores", request));
            }
            await Task.WhenAll(tasks);

            // Act
            var response = await _client.GetAsync($"/api/highscores?count=3&gameMode={gameMode}");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<HighScore>>>();
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal(3, apiResponse.Data.Count);

            // Verify scores are sorted correctly (highest first)
            Assert.Equal(1400, apiResponse.Data[0].Score);
            Assert.Equal(1300, apiResponse.Data[1].Score);
            Assert.Equal(1200, apiResponse.Data[2].Score);
        }

        [Fact]
        public async Task CheckHighScore_HighScore_ReturnsTrue()
        {
            // Arrange
            var gameMode = "TestCheck_" + Guid.NewGuid().ToString("N")[..8];
            TrackGameMode(gameMode);

            var saveRequest = new
            {
                PlayerInitials = "LOW",
                Score = 500,
                GameMode = gameMode
            };
            await _client.PostAsJsonAsync("/api/highscores", saveRequest);

            // Act - Check if 1000 is a high score
            var response = await _client.GetAsync($"/api/highscores/check/1000?gameMode={gameMode}");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.True(apiResponse.Data);
        }

        [Fact]
        public async Task CheckHighScore_NotHighScore_ReturnsFalse()
        {
            // Arrange
            var gameMode = "TestFull_" + Guid.NewGuid().ToString("N")[..8];
            TrackGameMode(gameMode);

            var tasks = new List<Task>();
            for (int i = 0; i < 12; i++) // More than 10 to fill the leaderboard
            {
                var request = new
                {
                    PlayerInitials = $"H{i:D2}",
                    Score = 2000 + (i * 100),
                    GameMode = gameMode
                };
                tasks.Add(_client.PostAsJsonAsync("/api/highscores", request));
            }
            await Task.WhenAll(tasks);

            // Act - Check if 1000 is a high score (it shouldn't be)
            var response = await _client.GetAsync($"/api/highscores/check/1000?gameMode={gameMode}");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.False(apiResponse.Data);
        }

        [Fact]
        public async Task GetPlayerRank_ValidScore_ReturnsCorrectRank()
        {
            // Arrange
            var gameMode = "TestRank_" + Guid.NewGuid().ToString("N")[..8];
            TrackGameMode(gameMode);

            var scores = new[] { 3000, 2500, 2000, 1500, 1000 };
            var tasks = new List<Task>();

            for (int i = 0; i < scores.Length; i++)
            {
                var request = new
                {
                    PlayerInitials = $"R{i:D2}",
                    Score = scores[i],
                    GameMode = gameMode
                };
                tasks.Add(_client.PostAsJsonAsync("/api/highscores", request));
            }
            await Task.WhenAll(tasks);

            // Act - Check rank for score 1750 (should be rank 4)
            var response = await _client.GetAsync($"/api/highscores/rank/1750?gameMode={gameMode}");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse.Success);
            Assert.Equal(4, apiResponse.Data);
        }

        [Fact]
        public async Task HighScoreService_CompleteFlow_WorksEndToEnd()
        {
            // Arrange
            var gameMode = "TestE2E_" + Guid.NewGuid().ToString("N")[..8];
            TrackGameMode(gameMode);

            using var scope = _factory.Services.CreateScope();
            var highScoreService = scope.ServiceProvider.GetRequiredService<IHighScoreService>();

            // Act & Assert - Save a high score
            var saveResult = await highScoreService.SaveHighScoreAsync("E2E", 1800, gameMode);
            Assert.True(saveResult);

            // Act & Assert - Get top scores
            var topScores = await highScoreService.GetTopScoresAsync(10, gameMode);
            Assert.NotNull(topScores);
            Assert.Contains(topScores, s => s.PlayerInitials == "E2E" && s.Score == 1800);

            // Act & Assert - Check if it's a high score
            var isHighScore = await highScoreService.IsHighScoreAsync(1900, gameMode);
            Assert.True(isHighScore);

            // Act & Assert - Get player rank
            var rank = await highScoreService.GetPlayerRankAsync(1900, gameMode);
            Assert.Equal(1, rank);
        }

        [Theory]
        [InlineData("")]
        [InlineData("AB")]
        [InlineData("ABCD")]
        [InlineData(null)]
        public async Task SaveHighScore_InvalidInitials_ReturnsFalse(string? initials)
        {
            // Arrange
            var gameMode = "TestInvalidInit_" + Guid.NewGuid().ToString("N")[..8];
            TrackGameMode(gameMode);

            using var scope = _factory.Services.CreateScope();
            var highScoreService = scope.ServiceProvider.GetRequiredService<IHighScoreService>();

            // Act
            var result = await highScoreService.SaveHighScoreAsync(initials, 1000, gameMode);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HighScoreService_MultipleGameModes_KeepsSeparateLeaderboards()
        {
            // Arrange
            var easyMode = "TestEasy_" + Guid.NewGuid().ToString("N")[..8];
            var hardMode = "TestHard_" + Guid.NewGuid().ToString("N")[..8];
            TrackGameMode(easyMode);
            TrackGameMode(hardMode);

            using var scope = _factory.Services.CreateScope();
            var highScoreService = scope.ServiceProvider.GetRequiredService<IHighScoreService>();

            // Act - Save scores in different game modes
            await highScoreService.SaveHighScoreAsync("EZ1", 1000, easyMode);
            await highScoreService.SaveHighScoreAsync("HD1", 2000, hardMode);
            await highScoreService.SaveHighScoreAsync("EZ2", 1200, easyMode);

            // Assert - Check that each mode has its own leaderboard
            var easyScores = await highScoreService.GetTopScoresAsync(10, easyMode);
            var hardScores = await highScoreService.GetTopScoresAsync(10, hardMode);

            Assert.Equal(2, easyScores.Count);
            Assert.Single(hardScores);
            Assert.All(easyScores, s => Assert.Equal(easyMode, s.GameMode));
            Assert.All(hardScores, s => Assert.Equal(hardMode, s.GameMode));
        }
    }

