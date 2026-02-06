using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Azure.Data.Tables;
using Testcontainers.Azurite;
using Xunit;

namespace PoBabyTouchGc.Tests.Integration;

/// <summary>
/// Custom <see cref="WebApplicationFactory{TEntryPoint}"/> that spins up an
/// Azurite container via Testcontainers so integration tests run against a
/// real (ephemeral) Azure Table Storage emulator.
/// Implements <see cref="IAsyncLifetime"/> so xUnit manages start/stop.
/// </summary>
public sealed class AzuriteWebApplicationFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly AzuriteContainer _azurite = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
        .Build();

    public string AzuriteConnectionString => _azurite.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove the existing TableServiceClient registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(TableServiceClient));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            // Register a new TableServiceClient pointing at the Azurite container
            services.AddSingleton<TableServiceClient>(_ =>
            {
                var client = new TableServiceClient(_azurite.GetConnectionString());
                // Pre-create the tables used by the app
                client.GetTableClient("PoBabyTouchHighScores").CreateIfNotExists();
                client.GetTableClient("PoBabyTouchGcGameStats").CreateIfNotExists();
                return client;
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _azurite.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _azurite.DisposeAsync();
    }
}
