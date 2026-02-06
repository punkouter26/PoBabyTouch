using Xunit;

namespace PoBabyTouchGc.Tests.Integration;

/// <summary>
/// Shared test collection that ensures all integration test classes
/// reuse a single <see cref="AzuriteWebApplicationFactory"/> (and therefore a
/// single Azurite container), cutting container startup overhead.
/// </summary>
[CollectionDefinition(Name)]
public sealed class AzuriteCollection : ICollectionFixture<AzuriteWebApplicationFactory>
{
    public const string Name = "Azurite";
}
