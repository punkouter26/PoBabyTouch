using PoBabyTouchGc.Shared.Models;
using MediatR;
using Microsoft.ApplicationInsights;

namespace PoBabyTouchGc.Api.Features.HighScores;

// Applying CQRS Pattern - Query
public record GetTopScoresQuery(int Count, string GameMode) : IRequest<List<HighScore>>;

public class GetTopScoresHandler : IRequestHandler<GetTopScoresQuery, List<HighScore>>
{
    private readonly IHighScoreRepository _repository;
    private readonly ILogger<GetTopScoresHandler> _logger;
    private readonly TelemetryClient _telemetryClient;

    public GetTopScoresHandler(
        IHighScoreRepository repository,
        ILogger<GetTopScoresHandler> logger,
        TelemetryClient telemetryClient)
    {
        _repository = repository;
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    public async Task<List<HighScore>> Handle(GetTopScoresQuery request, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        _logger.LogDebug("Getting top {Count} scores for {GameMode} mode", request.Count, request.GameMode);

        // Application Insights: Track custom event for leaderboard views
        _telemetryClient.TrackEvent("LeaderboardViewed", new Dictionary<string, string>
        {
            { "GameMode", request.GameMode },
            { "Count", request.Count.ToString() }
        });

        var scores = await _repository.GetTopScoresAsync(request.Count, request.GameMode);

        // Telemetry: Track performance
        var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
        _telemetryClient.TrackMetric("LeaderboardLoadTime", duration, new Dictionary<string, string>
        {
            { "GameMode", request.GameMode },
            { "ScoreCount", scores.Count.ToString() }
        });

        _logger.LogInformation("GetTopScores completed in {Duration}ms, returned {ScoreCount} scores",
            duration, scores.Count);

        return scores;
    }
}

// Minimal API endpoint configuration
public static class GetTopScoresEndpoint
{
    public static IEndpointRouteBuilder MapGetTopScoresEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/highscores", async (
            int count,
            string gameMode,
            IMediator mediator) =>
        {
            try
            {
                var scores = await mediator.Send(new GetTopScoresQuery(count > 0 ? count : 10, gameMode ?? "Default"));
                return Results.Ok(ApiResponse<List<HighScore>>.SuccessResult(scores));
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Failed to retrieve top scores");
            }
        })
        .WithName("GetTopScores")
        .WithTags("HighScores")
        .Produces<ApiResponse<List<HighScore>>>(200)
        .Produces(500);

        return app;
    }
}
