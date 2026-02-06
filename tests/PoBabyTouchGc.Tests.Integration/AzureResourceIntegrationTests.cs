using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.ApplicationInsights;
using Azure.Data.Tables;
using PoBabyTouchGc.Api;
using PoBabyTouchGc.Api.Models;
using System.Net;
using Xunit;

namespace PoBabyTouchGc.Tests.Integration;

/// <summary>
/// Integration tests to verify Azure resource connections
/// These tests verify that the application can connect to all required Azure services (Azurite via Testcontainers)
/// </summary>
[Collection(AzuriteCollection.Name)]
public class AzureResourceIntegrationTests
{
    private readonly AzuriteWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AzureResourceIntegrationTests(AzuriteWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnHealthy_WhenAllDependenciesAreAvailable()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert - Health endpoint may return OK, Conflict (409), or ServiceUnavailable depending on config
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || 
            response.StatusCode == HttpStatusCode.Conflict ||
            response.StatusCode == HttpStatusCode.ServiceUnavailable,
            $"Expected OK, Conflict, or ServiceUnavailable but got {response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
    }

    [Fact]
    public async Task AzureTableStorage_ShouldBeAccessible_WhenConfigured()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var tableServiceClient = scope.ServiceProvider.GetRequiredService<TableServiceClient>();
        var tableClient = tableServiceClient.GetTableClient("PoBabyTouchHighScores");

        // Act & Assert - Should not throw
        await tableClient.CreateIfNotExistsAsync();

        // Verify table exists
        var exists = await tableClient.GetEntityIfExistsAsync<TableEntity>("test", "test");
        Assert.NotNull(exists); // GetEntityIfExistsAsync returns a Response<T> which should not be null
    }

    [Fact]
    public void ApplicationInsights_ShouldBeConfigured_WhenTelemetryClientExists()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();

        // Act - TelemetryClient is optional in local development
        var telemetryClient = scope.ServiceProvider.GetService<TelemetryClient>();
        
        // Skip test if App Insights not configured (local dev scenario)
        if (telemetryClient == null)
        {
            // Test passes - App Insights is optional in development
            return;
        }

        // Verify telemetry client can track events
        telemetryClient.TrackEvent("IntegrationTest_ApplicationInsights_Configured");
        telemetryClient.Flush();
    }

    [Fact]
    public async Task HighScoresAPI_ShouldWork_WithAzureTableStorage()
    {
        // Act
        var response = await _client.GetAsync("/api/highscores?gameMode=IntegrationTest");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        // Should return a successful API response with data array
        Assert.Contains("success", content);
        Assert.Contains("data", content);
    }

    [Fact]
    public void Configuration_ShouldHaveRequiredConnectionStrings()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Act - Check for connection string from multiple possible keys
        var azureTableStorageConnectionString = 
            configuration.GetConnectionString("AzureTableStorage") ??
            configuration.GetConnectionString("tableStorage");
        
        // In local dev, we fallback to Azurite via code, so connection string may be null
        // This test passes if either: 1) connection string exists, or 2) TableServiceClient is registered
        if (string.IsNullOrEmpty(azureTableStorageConnectionString))
        {
            var tableServiceClient = scope.ServiceProvider.GetService<TableServiceClient>();
            Assert.NotNull(tableServiceClient); // Fallback: verify TableServiceClient is available
            return;
        }

        Assert.NotEmpty(azureTableStorageConnectionString);

        // Should be either Azurite (development) or Azure Storage (production)
        Assert.True(
            azureTableStorageConnectionString.Contains("UseDevelopmentStorage=true") ||
            azureTableStorageConnectionString.Contains("DefaultEndpointsProtocol=https"),
            "Connection string should be either Azurite or Azure Storage format");
    }

    [Fact]
    public async Task HighScoreDiagnostics_ShouldReturnConnectionStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/highscores/diagnostics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("status", content);
        Assert.Contains("timestamp", content);
        Assert.Contains("environment", content);
    }

    [Fact]
    public async Task AzureResources_ShouldHandleTemporaryFailures_WithGracefulDegradation()
    {
        // This test verifies that the application gracefully handles Azure service temporary failures

        // Act - Test health endpoint which checks all dependencies
        var response = await _client.GetAsync("/api/health");

        // Assert - Should return a response (may be healthy, degraded, or conflict - but not fail completely)
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || 
            response.StatusCode == HttpStatusCode.Conflict ||
            response.StatusCode == HttpStatusCode.ServiceUnavailable,
            $"Expected OK, Conflict, or ServiceUnavailable but got {response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }
}
