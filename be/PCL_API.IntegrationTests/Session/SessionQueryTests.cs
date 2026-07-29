using System.Net;
using System.Net.Http.Json;
using PCL_API.IntegrationTests.Infrastructure;
using PCL_API.IntegrationTests.TestData;
using Xunit;

namespace PCL_API.IntegrationTests.Session;

public sealed class SessionQueryTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Start_get_and_list_should_return_created_sessions()
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        DateTimeOffset startedAt = DateTimeOffset.UtcNow.AddMinutes(-10);
        Guid sessionId = await StartSessionAsync(ownerId, "Focused practice", startedAt);
        await StartSessionAsync(TestDataBuilder.NewOwnerId(), "Another owner's session");

        SessionResponse session = await GetSessionAsync(sessionId);
        Assert.Equal(ownerId, session.OwnerId);
        Assert.Equal("Focused practice", session.Title);
        Assert.Equal(0, session.Status);
        Assert.Empty(session.AssignedTaskIds);
        Assert.InRange((session.StartedAt - startedAt).Duration(), TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

        IReadOnlyCollection<SessionResponse>? sessions = await Client.GetFromJsonAsync<IReadOnlyCollection<SessionResponse>>(
            "/lsessions", TestContext.Current.CancellationToken);
        Assert.NotNull(sessions);
        Assert.Contains(sessions, item => item.Id == sessionId);
        Assert.Equal(2, sessions.Count);
    }

    [Fact]
    public async Task Get_should_report_a_missing_session()
    {
        HttpResponseMessage response = await Client.GetAsync(
            $"/lsessions/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, "LSession.NotFound");
    }

    private async Task<Guid> StartSessionAsync(Guid ownerId, string title, DateTimeOffset? startedAt = null)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/lsessions", TestDataBuilder.StartSessionRequest(ownerId, title, startedAt),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private async Task<SessionResponse> GetSessionAsync(Guid sessionId) =>
        await Client.GetFromJsonAsync<SessionResponse>(
            $"/lsessions/{sessionId}", TestContext.Current.CancellationToken)
        ?? throw new Xunit.Sdk.XunitException("The session response was empty.");
}
