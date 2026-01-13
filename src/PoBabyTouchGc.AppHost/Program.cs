var builder = DistributedApplication.CreateBuilder(args);

// Add Azure Storage (using Azurite for local development)
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();

var tableStorage = storage.AddTables("tableStorage");

// Add the Web project (Blazor Server with Interactive Auto + API endpoints)
var web = builder.AddProject<Projects.PoBabyTouchGc_Web>("web")
    .WithExternalHttpEndpoints()
    .WithReference(tableStorage)
    .WaitFor(tableStorage)
    .WithHttpHealthCheck("/api/health");

builder.Build().Run();
