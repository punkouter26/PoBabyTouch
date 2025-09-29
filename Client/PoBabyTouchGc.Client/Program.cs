using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PoBabyTouchGc.Client;
using PoBabyTouchGc.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<GameStateService>();

// Configure custom server logging
builder.Logging.ClearProviders();

// Add custom server logger provider
builder.Services.AddSingleton<ServerLoggerProvider>(serviceProvider =>
{
    var httpClient = serviceProvider.GetRequiredService<HttpClient>();
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    return new ServerLoggerProvider(httpClient, configuration);
});

// Register the custom logger provider
builder.Services.AddSingleton<Microsoft.Extensions.Logging.ILoggerProvider>(serviceProvider =>
    serviceProvider.GetRequiredService<ServerLoggerProvider>());

var app = builder.Build();

// Initialize telemetry logging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogGameSession("ApplicationStarted", "Client", new Dictionary<string, object>
{
    {"StartupTime", DateTime.UtcNow},
    {"BaseAddress", builder.HostEnvironment.BaseAddress},
    {"Environment", builder.HostEnvironment.Environment}
});

await app.RunAsync();
