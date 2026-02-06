using Azure.Data.Tables;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using PoBabyTouchGc.Api.Features.HighScores;
using PoBabyTouchGc.Api.Features.Statistics;
using PoBabyTouchGc.Api.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Azure Key Vault ──────────────────────────────────────────────────────────
// Secrets are injected via Key Vault references in App Service app settings
// (e.g. @Microsoft.KeyVault(VaultName=…;SecretName=…)).
// No AddAzureKeyVault call is needed — loading all secrets from a shared vault
// causes cross-app config collisions (e.g. ConnectionStrings--AzureTableStorage
// from another app overriding the correct value).

// ── Serilog ──────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

// ── OpenTelemetry ────────────────────────────────────────────────────────────
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();
    })
    .WithTracing(tracing =>
    {
        tracing.AddSource(builder.Environment.ApplicationName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    });

var useOtlpExporter = !string.IsNullOrWhiteSpace(
    builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
if (useOtlpExporter)
{
    builder.Services.AddOpenTelemetry().UseOtlpExporter();
}

// ── Health Checks ────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

// ── Controllers & OpenAPI / Swagger ──────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Azure Table Storage ──────────────────────────────────────────────────────
string? storageConnectionString =
    builder.Configuration.GetConnectionString("AzureTableStorage");

if (string.IsNullOrWhiteSpace(storageConnectionString) && builder.Environment.IsDevelopment())
{
    storageConnectionString = "UseDevelopmentStorage=true";
}

Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);

if (!string.IsNullOrWhiteSpace(storageConnectionString))
{
    Log.Information("Using {StorageType} for Azure Table Storage",
        storageConnectionString == "UseDevelopmentStorage=true" ? "Azurite (local)" : "Azure Storage");

    builder.Services.AddSingleton<TableServiceClient>(sp =>
    {
        var tableServiceClient = new TableServiceClient(storageConnectionString);
        try
        {
            var tableClient = tableServiceClient.GetTableClient("PoBabyTouchHighScores");
            tableClient.CreateIfNotExists();
            Log.Information("Successfully initialized TableServiceClient");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to verify table setup during initialization");
        }
        return tableServiceClient;
    });
}
else
{
    Log.Warning("No Azure Table Storage connection string configured — running without persistence");
}

// ── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:5173", "http://localhost:3000"])
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// ── Application Services (Repository / Strategy / Service Layer Patterns) ───
builder.Services.AddScoped<IHighScoreRepository, AzureTableHighScoreRepository>();
builder.Services.AddScoped<IHighScoreValidationService, HighScoreValidationService>();
builder.Services.AddScoped<IHighScoreService, HighScoreService>();
builder.Services.AddScoped<IGameStatsRepository, AzureTableGameStatsRepository>();

var app = builder.Build();

// ── Middleware Pipeline ──────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PoBabyTouchGc API V1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseHsts();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

// ── Map Endpoints ────────────────────────────────────────────────────────────
app.MapControllers();
app.MapStatisticsEndpoints();

// ── Health check endpoints ───────────────────────────────────────────────────
app.MapHealthChecks("/health");
app.MapHealthChecks("/alive", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live")
});

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
