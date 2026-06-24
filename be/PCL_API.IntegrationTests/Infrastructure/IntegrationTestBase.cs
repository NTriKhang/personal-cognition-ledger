using Xunit;

namespace PCL_API.IntegrationTests.Infrastructure;

[Collection(IntegrationTestCollection.Name)]
public abstract class IntegrationTestBase(IntegrationTestFixture fixture)
    : IAsyncLifetime
{
    protected IntegrationTestFixture Fixture { get; } = fixture;
    protected HttpClient Client => Fixture.Client;

    public ValueTask InitializeAsync() =>
        new(Fixture.ResetDatabaseAsync());

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
