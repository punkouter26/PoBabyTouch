using Azure.Data.Tables;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using PoBabyTouchGc.Client.Services;
using PoBabyTouchGc.Web.Components;
using PoBabyTouchGc.Web.Features.HighScores;
using PoBabyTouchGc.Web.Features.Statistics;
using PoBabyTouchGc.Web.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

// Add Aspire service defaults (OpenTelemetry, health checks, service discovery)
builder.AddServiceDefaults();

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Add FluentUI components
builder.Services.AddFluentUIComponents();

// Add API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Azure Table Storage
// Aspire injects the connection string as "ConnectionStrings__tableStorage" env var
string? storageConnectionString = builder.Configuration.GetConnectionString("tableStorage")
    ?? builder.Configuration.GetConnectionString("AzureTableStorage");

// For development, fallback to Azurite
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
    Log.Warning("No Azure Table Storage connection string configured");
}

// Add CORS policy for client
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add API services (Repository Pattern, Strategy Pattern, Service Layer Pattern)
builder.Services.AddScoped<IHighScoreRepository, AzureTableHighScoreRepository>();
builder.Services.AddScoped<IHighScoreValidationService, HighScoreValidationService>();
builder.Services.AddScoped<IHighScoreService, PoBabyTouchGc.Web.Features.HighScores.HighScoreService>();
builder.Services.AddScoped<IGameStatsRepository, AzureTableGameStatsRepository>();

// Register services for Blazor components
builder.Services.AddScoped<GameStateService>();
builder.Services.AddScoped<IGamePhysicsEngine, StandardPhysicsEngine>();
builder.Services.AddScoped<CircleManager>();
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<PoBabyTouchGc.Client.Services.HighScoreService>();
builder.Services.AddScoped<GameStatsService>();

// HttpClient for client-side API calls (points to self)
builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) };
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PoBabyTouchGc API V1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Global Exception Handler Middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseCors("CorsPolicy");

app.MapStaticAssets();

// Map API controllers and endpoints
app.MapControllers();
app.MapStatisticsEndpoints();

// Map Blazor components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(PoBabyTouchGc.Client._Imports).Assembly);

// Map Aspire default endpoints (health checks)
app.MapDefaultEndpoints();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
