using System.Net;
using System.Net.Http.Json;
using PCL_API.IntegrationTests.Infrastructure;
using PCL_API.IntegrationTests.TestData;
using Xunit;

namespace PCL_API.IntegrationTests.Foundation;

public sealed class ApiFoundationTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    // Verifies the API starts and can read from the migrated test database.
    [Fact]
    public async Task Api_host_should_serve_requests_against_migrated_database()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        HttpResponseMessage response = await Client.GetAsync(
            "/lsessions",
            cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        IReadOnlyCollection<object>? sessions =
            await response.Content.ReadFromJsonAsync<IReadOnlyCollection<object>>(
                cancellationToken);

        Assert.NotNull(sessions);
        Assert.Empty(sessions);
    }

    // Verifies database cleanup removes data created by a previous scenario.
    [Fact]
    public async Task Database_reset_should_remove_test_data_between_scenarios()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Guid ownerId = TestDataBuilder.NewOwnerId();

        HttpResponseMessage createResponse = await Client.PostAsJsonAsync(
            "/lsessions",
            TestDataBuilder.StartSessionRequest(ownerId),
            cancellationToken);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        await Fixture.ResetDatabaseAsync();

        HttpResponseMessage listResponse = await Client.GetAsync(
            "/lsessions",
            cancellationToken);
        IReadOnlyCollection<object>? sessions =
            await listResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<object>>(
                cancellationToken);

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.NotNull(sessions);
        Assert.Empty(sessions);
    }
}
