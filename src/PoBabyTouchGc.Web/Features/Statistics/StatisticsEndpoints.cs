using Microsoft.AspNetCore.Mvc;
using PoBabyTouchGc.Web.Features.Statistics;
using PoBabyTouchGc.Shared.Models;

namespace PoBabyTouchGc.Web.Features.Statistics;

/// <summary>
/// API endpoints for game statistics
/// Applying Minimal API pattern
/// </summary>
public static class StatisticsEndpoints
{
    public static void MapStatisticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/stats")
            .WithTags("Statistics");

        // GET /api/stats/{initials}
        group.MapGet("/{initials}", async (
            string initials,
            IGameStatsRepository repository,
            ILogger<Program> logger) =>
        {
            try
            {
                var stats = await repository.GetStatsAsync(initials);
                
                if (stats == null)
                {
                    return Results.Ok(new ApiResponse<GameStats>
                    {
                        Success = false,
                        Message = "No statistics found for these initials",
                        Data = null
                    });
                }
                
                return Results.Ok(new ApiResponse<GameStats>
                {
                    Success = true,
                    Message = "Statistics retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving stats for {Initials}", initials);
                return Results.Ok(new ApiResponse<GameStats>
                {
                    Success = false,
                    Message = $"Error retrieving statistics: {ex.Message}",
                    Data = null
                });
            }
        })
        .WithName("GetPlayerStats")
        .WithSummary("Get statistics for a specific player");

        // GET /api/stats
        group.MapGet("/", async (
            IGameStatsRepository repository,
            ILogger<Program> logger) =>
        {
            try
            {
                var allStats = await repository.GetAllStatsAsync();
                
                return Results.Ok(new ApiResponse<IEnumerable<GameStats>>
                {
                    Success = true,
                    Message = $"Retrieved {allStats.Count()} player statistics",
                    Data = allStats
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all stats");
                return Results.Ok(new ApiResponse<IEnumerable<GameStats>>
                {
                    Success = false,
                    Message = $"Error retrieving statistics: {ex.Message}",
                    Data = Enumerable.Empty<GameStats>()
                });
            }
        })
        .WithName("GetAllStats")
        .WithSummary("Get statistics for all players");

        // POST /api/stats/record
        group.MapPost("/record", async (
            [FromBody] RecordGameRequest request,
            IGameStatsRepository repository,
            ILogger<Program> logger) =>
        {
            try
            {
                var stats = await repository.RecordGameSessionAsync(
                    request.Initials,
                    request.Score,
                    request.CirclesTapped,
                    request.PlaytimeSeconds);
                
                return Results.Ok(new ApiResponse<GameStats>
                {
                    Success = true,
                    Message = "Game session recorded successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error recording game session");
                return Results.Ok(new ApiResponse<GameStats>
                {
                    Success = false,
                    Message = $"Error recording game session: {ex.Message}",
                    Data = null
                });
            }
        })
        .WithName("RecordGameSession")
        .WithSummary("Record a game session and update statistics");
    }
}

/// <summary>
/// Request model for recording a game session
/// </summary>
public class RecordGameRequest
{
    public string Initials { get; set; } = string.Empty;
    public int Score { get; set; }
    public int CirclesTapped { get; set; }
    public int PlaytimeSeconds { get; set; }
}
