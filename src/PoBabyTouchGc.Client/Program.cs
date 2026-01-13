using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using PoBabyTouchGc.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<GameStateService>();

// Add game services (Applying Strategy Pattern for physics)
builder.Services.AddScoped<IGamePhysicsEngine, StandardPhysicsEngine>(); // Default to standard physics
builder.Services.AddScoped<CircleManager>();
builder.Services.AddScoped<HighScoreService>();
builder.Services.AddScoped<GameStatsService>();
builder.Services.AddScoped<ApiClient>();

// Add FluentUI services
builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();
