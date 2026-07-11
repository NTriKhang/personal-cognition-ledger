using System.Net;
using System.Net.Http.Json;
using PCL_API.IntegrationTests.Infrastructure;
using PCL_API.IntegrationTests.TestData;
using Xunit;

namespace PCL_API.IntegrationTests.Session;

public sealed class SessionLifecycleTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Owner_should_have_only_one_active_session()
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        await StartSessionAsync(ownerId);

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/lsessions", TestDataBuilder.StartSessionRequest(ownerId), TestContext.Current.CancellationToken);

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, "LSession.ActiveSessionAlreadyExists");
    }

    [Fact]
    public async Task End_should_stop_the_session_and_reject_repeated_ending()
    {
        Guid sessionId = await StartSessionAsync(TestDataBuilder.NewOwnerId());
        DateTimeOffset endedAt = DateTimeOffset.UtcNow;

        HttpResponseMessage ended = await Client.PutAsJsonAsync(
            $"/lsessions/{sessionId}/end", TestDataBuilder.EndSessionRequest(endedAt),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, ended.StatusCode);

        SessionResponse? session = await Client.GetFromJsonAsync<SessionResponse>(
            $"/lsessions/{sessionId}", TestContext.Current.CancellationToken);
        Assert.NotNull(session);
        Assert.Equal(1, session.Status);
        Assert.NotNull(session.EndedAt);
        Assert.InRange(
            (session.EndedAt.Value - endedAt).Duration(),
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(1));

        HttpResponseMessage repeated = await Client.PutAsJsonAsync(
            $"/lsessions/{sessionId}/end", TestDataBuilder.EndSessionRequest(),
            TestContext.Current.CancellationToken);
        await repeated.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "LSession.AlreadyEnded");
    }

    [Fact]
    public async Task End_should_reject_missing_session_and_time_before_start()
    {
        DateTimeOffset startedAt = DateTimeOffset.UtcNow;
        Guid sessionId = await StartSessionAsync(TestDataBuilder.NewOwnerId(), startedAt);

        HttpResponseMessage invalidTime = await Client.PutAsJsonAsync(
            $"/lsessions/{sessionId}/end", TestDataBuilder.EndSessionRequest(startedAt.AddSeconds(-1)),
            TestContext.Current.CancellationToken);
        await invalidTime.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "LSession.InvalidEndTime");

        HttpResponseMessage missing = await Client.PutAsJsonAsync(
            $"/lsessions/{Guid.NewGuid()}/end", TestDataBuilder.EndSessionRequest(),
            TestContext.Current.CancellationToken);
        await missing.ShouldBeProblemAsync(HttpStatusCode.NotFound, "LSession.NotFound");
    }

    private async Task<Guid> StartSessionAsync(Guid ownerId, DateTimeOffset? startedAt = null)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/lsessions", TestDataBuilder.StartSessionRequest(ownerId, startedAt: startedAt),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }
}
