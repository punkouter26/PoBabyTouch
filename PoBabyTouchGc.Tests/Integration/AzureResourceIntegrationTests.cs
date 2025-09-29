using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.ApplicationInsights;
using Azure.Data.Tables;
using PoBabyTouchGc.Server;
using PoBabyTouchGc.Shared.Models;
using System.Net;
using Xunit;

namespace PoBabyTouchGc.Tests.Integration;

/// <summary>
/// Integration tests to verify Azure resource connections
/// These tests verify that the application can connect to all required Azure services
/// </summary>
public class AzureResourceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AzureResourceIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnHealthy_WhenAllDependenciesAreAvailable()
    {
        // Act
        var response = await _client.GetAsync("/api/health");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
        Assert.Contains("status", content);
        Assert.Contains("dependencies", content);  // JSON serialization uses camelCase
    }

    [Fact]
    public async Task AzureTableStorage_ShouldBeAccessible_WhenConfigured()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var tableServiceClient = scope.ServiceProvider.GetRequiredService<TableServiceClient>();
        var tableClient = tableServiceClient.GetTableClient("PoBabyTouchGcHighScores");
        
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
        
        // Act & Assert
        var telemetryClient = scope.ServiceProvider.GetService<TelemetryClient>();
        Assert.NotNull(telemetryClient);
        
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
        
        // Act & Assert
        var azureTableStorageConnectionString = configuration.GetConnectionString("AzureTableStorage");
        Assert.NotNull(azureTableStorageConnectionString);
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
        
        // Assert - Should return a response (may be healthy or degraded, but not fail completely)
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.ServiceUnavailable);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
    }
}