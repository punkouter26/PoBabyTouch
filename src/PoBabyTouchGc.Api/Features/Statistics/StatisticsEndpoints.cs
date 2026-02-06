using Microsoft.AspNetCore.Mvc;
using PoBabyTouchGc.Api.Features.Statistics;
using PoBabyTouchGc.Api.Models;

namespace PoBabyTouchGc.Api.Features.Statistics;

/// <summary>
/// Minimal API endpoints for game statistics (Vertical Slice Architecture).
/// </summary>
public static class StatisticsEndpoints
{
    public static void MapStatisticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stats").WithTags("Statistics");

        group.MapGet("/{initials}", async (
            string initials,
            IGameStatsRepository repository,
            ILogger<Program> logger) =>
        {
            try
            {
                var stats = await repository.GetStatsAsync(initials);
                return stats == null
                    ? Results.Ok(ApiResponse<GameStats>.ErrorResult("No statistics found for these initials"))
                    : Results.Ok(ApiResponse<GameStats>.SuccessResult(stats));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving stats for {Initials}", initials);
                return Results.Ok(ApiResponse<GameStats>.ErrorResult($"Error: {ex.Message}"));
            }
        })
        .WithName("GetPlayerStats")
        .WithSummary("Get statistics for a specific player");

        group.MapGet("/", async (
            IGameStatsRepository repository,
            ILogger<Program> logger) =>
        {
            try
            {
                var allStats = await repository.GetAllStatsAsync();
                return Results.Ok(ApiResponse<IEnumerable<GameStats>>.SuccessResult(allStats,
                    $"Retrieved {allStats.Count()} player statistics"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all stats");
                return Results.Ok(ApiResponse<IEnumerable<GameStats>>.ErrorResult($"Error: {ex.Message}"));
            }
        })
        .WithName("GetAllStats")
        .WithSummary("Get statistics for all players");

        group.MapPost("/record", async (
            [FromBody] RecordGameRequest request,
            IGameStatsRepository repository,
            ILogger<Program> logger) =>
        {
            try
            {
                var stats = await repository.RecordGameSessionAsync(
                    request.Initials, request.Score, request.CirclesTapped, request.PlaytimeSeconds);
                return Results.Ok(ApiResponse<GameStats>.SuccessResult(stats, "Game session recorded"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error recording game session");
                return Results.Ok(ApiResponse<GameStats>.ErrorResult($"Error: {ex.Message}"));
            }
        })
        .WithName("RecordGameSession")
        .WithSummary("Record a game session and update statistics");
    }
}

public class RecordGameRequest
{
    public string Initials { get; set; } = string.Empty;
    public int Score { get; set; }
    public int CirclesTapped { get; set; }
    public int PlaytimeSeconds { get; set; }
}
