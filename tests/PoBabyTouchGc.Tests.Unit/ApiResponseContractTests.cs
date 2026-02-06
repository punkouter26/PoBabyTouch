using System.Text.Json;
using PoBabyTouchGc.Api.Models;
using Xunit;

namespace PoBabyTouchGc.Tests.Unit;

/// <summary>
/// Contract tests that verify serialised API response shapes match
/// the TypeScript interfaces consumed by the React client.
/// Prevents silent breaking changes when model properties are renamed,
/// reordered, or removed on the .NET side.
/// </summary>
public class ApiResponseContractTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    // ─── ApiResponse<T> envelope ─────────────────────────────────────────

    [Fact]
    public void ApiResponse_Success_ContainsExpectedJsonProperties()
    {
        var response = ApiResponse<string>.SuccessResult("test-data", "OK");
        var json = JsonSerializer.Serialize(response, JsonOptions);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("success", out var success));
        Assert.True(success.GetBoolean());

        Assert.True(root.TryGetProperty("message", out var message));
        Assert.Equal("OK", message.GetString());

        Assert.True(root.TryGetProperty("data", out var data));
        Assert.Equal("test-data", data.GetString());

        Assert.True(root.TryGetProperty("errorCode", out _));
        Assert.True(root.TryGetProperty("timestamp", out _));
    }

    [Fact]
    public void ApiResponse_Error_ContainsExpectedJsonProperties()
    {
        var response = ApiResponse<object>.ErrorResult("something failed", "ERR_001");
        var json = JsonSerializer.Serialize(response, JsonOptions);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("success", out var success));
        Assert.False(success.GetBoolean());

        Assert.True(root.TryGetProperty("message", out var msg));
        Assert.Equal("something failed", msg.GetString());

        Assert.True(root.TryGetProperty("errorCode", out var code));
        Assert.Equal("ERR_001", code.GetString());

        Assert.True(root.TryGetProperty("data", out _));
        Assert.True(root.TryGetProperty("timestamp", out _));
    }

    [Fact]
    public void ApiResponse_HasExactlyFiveProperties()
    {
        // The React client expects exactly: success, message, data, errorCode, timestamp
        var response = ApiResponse<int>.SuccessResult(42);
        var json = JsonSerializer.Serialize(response, JsonOptions);
        var doc = JsonDocument.Parse(json);

        var propertyNames = new HashSet<string>();
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            propertyNames.Add(prop.Name);
        }

        Assert.Equal(5, propertyNames.Count);
        Assert.Contains("success", propertyNames);
        Assert.Contains("message", propertyNames);
        Assert.Contains("data", propertyNames);
        Assert.Contains("errorCode", propertyNames);
        Assert.Contains("timestamp", propertyNames);
    }

    // ─── HighScore entity ────────────────────────────────────────────────

    [Fact]
    public void HighScore_JsonShape_MatchesTypeScriptInterface()
    {
        // TypeScript interface expects:
        //   playerInitials, score, gameMode, scoreDate,
        //   partitionKey, rowKey, timestamp
        var hs = new HighScore("Default", "ABC", 2500);
        var json = JsonSerializer.Serialize(hs, JsonOptions);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("playerInitials", out var initials));
        Assert.Equal("ABC", initials.GetString());

        Assert.True(root.TryGetProperty("score", out var score));
        Assert.Equal(2500, score.GetInt32());

        Assert.True(root.TryGetProperty("gameMode", out var mode));
        Assert.Equal("Default", mode.GetString());

        Assert.True(root.TryGetProperty("scoreDate", out _));
        Assert.True(root.TryGetProperty("partitionKey", out _));
        Assert.True(root.TryGetProperty("rowKey", out _));
        Assert.True(root.TryGetProperty("timestamp", out _));
        Assert.True(root.TryGetProperty("eTag", out _));
    }

    [Fact]
    public void HighScore_WrappedInApiResponse_ProducesValidContract()
    {
        var hs = new HighScore("Easy", "XYZ", 1000);
        var wrapped = ApiResponse<List<HighScore>>.SuccessResult([hs]);
        var json = JsonSerializer.Serialize(wrapped, JsonOptions);
        var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("data", out var data));
        Assert.Equal(JsonValueKind.Array, data.ValueKind);
        Assert.Equal(1, data.GetArrayLength());

        var first = data[0];
        Assert.True(first.TryGetProperty("playerInitials", out _));
        Assert.True(first.TryGetProperty("score", out _));
        Assert.True(first.TryGetProperty("gameMode", out _));
    }

    // ─── GameStats model ─────────────────────────────────────────────────

    [Fact]
    public void GameStats_JsonShape_MatchesTypeScriptInterface()
    {
        // TypeScript interface expects:
        //   initials, totalGames, totalCirclesTapped, averageScore,
        //   highestScore, longestStreak, totalPlaytimeSeconds,
        //   lastPlayed, firstPlayed, percentileRank, scoreDistribution
        var stats = new GameStats
        {
            Initials = "TST",
            TotalGames = 10,
            TotalCirclesTapped = 500,
            AverageScore = 1500.5,
            HighestScore = 3000,
            LongestStreak = 5,
            TotalPlaytimeSeconds = 7200,
            LastPlayed = DateTime.UtcNow,
            FirstPlayed = DateTime.UtcNow.AddDays(-30),
            PercentileRank = 85.5,
            ScoreDistribution = "0-99:2,100-199:5",
        };

        var json = JsonSerializer.Serialize(stats, JsonOptions);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Verify every property the React client reads
        Assert.True(root.TryGetProperty("initials", out var initials));
        Assert.Equal("TST", initials.GetString());

        Assert.True(root.TryGetProperty("totalGames", out var tg));
        Assert.Equal(10, tg.GetInt32());

        Assert.True(root.TryGetProperty("totalCirclesTapped", out _));
        Assert.True(root.TryGetProperty("averageScore", out _));
        Assert.True(root.TryGetProperty("highestScore", out _));
        Assert.True(root.TryGetProperty("longestStreak", out _));
        Assert.True(root.TryGetProperty("totalPlaytimeSeconds", out _));
        Assert.True(root.TryGetProperty("lastPlayed", out _));
        Assert.True(root.TryGetProperty("firstPlayed", out _));
        Assert.True(root.TryGetProperty("percentileRank", out _));
        Assert.True(root.TryGetProperty("scoreDistribution", out var sd));
        Assert.Equal("0-99:2,100-199:5", sd.GetString());
    }

    [Fact]
    public void GameStats_WrappedInApiResponse_ProducesValidContract()
    {
        var stats = new GameStats { Initials = "ABC", TotalGames = 1 };
        var wrapped = ApiResponse<GameStats>.SuccessResult(stats);
        var json = JsonSerializer.Serialize(wrapped, JsonOptions);
        var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("success", out _));
        Assert.True(doc.RootElement.TryGetProperty("data", out var data));
        Assert.Equal(JsonValueKind.Object, data.ValueKind);
        Assert.True(data.TryGetProperty("initials", out _));
        Assert.True(data.TryGetProperty("totalGames", out _));
    }

    // ─── SaveHighScoreRequest ────────────────────────────────────────────

    [Fact]
    public void SaveHighScoreRequest_JsonShape_MatchesTypeScriptInterface()
    {
        var request = new SaveHighScoreRequest
        {
            PlayerInitials = "ABC",
            Score = 1500,
            GameMode = "Default",
        };

        var json = JsonSerializer.Serialize(request, JsonOptions);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("playerInitials", out _));
        Assert.True(root.TryGetProperty("score", out _));
        Assert.True(root.TryGetProperty("gameMode", out _));
    }
}
