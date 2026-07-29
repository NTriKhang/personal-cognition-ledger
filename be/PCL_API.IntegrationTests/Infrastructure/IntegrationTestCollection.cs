using Xunit;

namespace PCL_API.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection
    : ICollectionFixture<IntegrationTestFixture>
{
    public const string Name = "PCL API integration tests";
}
