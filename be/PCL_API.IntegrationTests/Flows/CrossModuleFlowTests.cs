using System.Net;
using System.Net.Http.Json;
using PCL_API.IntegrationTests.Evidence;
using PCL_API.IntegrationTests.Infrastructure;
using PCL_API.IntegrationTests.Infrastructure.Messaging;
using PCL_API.IntegrationTests.Session;
using PCL_API.IntegrationTests.TaskPlanning;
using PCL_API.IntegrationTests.TestData;
using Xunit;

namespace PCL_API.IntegrationTests.Flows;

public sealed class CrossModuleFlowTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    private static readonly MessageStore SessionMessages = new("Session", "session");
    private static readonly MessageStore TaskPlanningMessages = new("TaskPlanning", "task_planning");

    [Fact]
    public async Task Assignment_should_activate_task_and_removal_should_not_defer_it()
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid taskId = await CreatePlannedTaskAsync(ownerId);
        Guid sessionId = await StartSessionAsync(ownerId);

        HttpResponseMessage assigned = await Client.PutAsJsonAsync(
            $"/lsessions/{sessionId}/tasks/{taskId}",
            TestDataBuilder.AssignTaskRequest(),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, assigned.StatusCode);

        await Fixture.Messages.ProcessOutboxAsync(
            SessionMessages,
            TestContext.Current.CancellationToken);
        await Fixture.Messages.WaitForPendingInboxAsync(
            TaskPlanningMessages,
            TestContext.Current.CancellationToken);
        await Fixture.Messages.ProcessInboxAsync(
            TaskPlanningMessages,
            TestContext.Current.CancellationToken);
        Assert.Equal("Active", (await GetTaskAsync(taskId, ownerId)).Status);

        HttpResponseMessage removed = await Client.DeleteAsync(
            $"/lsessions/{sessionId}/tasks/{taskId}",
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, removed.StatusCode);
        Assert.DoesNotContain(taskId, (await GetSessionAsync(sessionId)).AssignedTaskIds);
        Assert.Equal("Active", (await GetTaskAsync(taskId, ownerId)).Status);
    }

    [Fact]
    public async Task Evidence_should_be_accepted_during_session_and_rejected_after_it_stops()
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid sessionId = await StartSessionAsync(ownerId);

        HttpResponseMessage added = await Client.PostAsJsonAsync(
            $"/lsessions/{sessionId}/evidence-items",
            TestDataBuilder.AddNoteEvidenceRequest(ownerId, "Cross-module evidence"),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, added.StatusCode);
        Guid evidenceId = await added.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

        IReadOnlyCollection<EvidenceItemResponse> evidence =
            await Client.GetFromJsonAsync<IReadOnlyCollection<EvidenceItemResponse>>(
                $"/lsessions/{sessionId}/evidence-items?ownerId={ownerId}",
                TestContext.Current.CancellationToken)
            ?? throw new Xunit.Sdk.XunitException("The evidence response was empty.");
        Assert.Contains(evidence, item => item.Id == evidenceId);

        await EndSessionAsync(sessionId);
        HttpResponseMessage rejected = await Client.PostAsJsonAsync(
            $"/lsessions/{sessionId}/evidence-items",
            TestDataBuilder.AddNoteEvidenceRequest(ownerId),
            TestContext.Current.CancellationToken);
        await rejected.ShouldBeProblemAsync(
            HttpStatusCode.Conflict, "SessionEvidenceAttachmentEligibility.NotActive");
    }

    [Fact]
    public async Task Task_completion_and_session_stop_should_remain_independent()
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid taskId = await CreatePlannedTaskAsync(ownerId);
        Guid sessionId = await StartSessionAsync(ownerId);

        HttpResponseMessage assigned = await Client.PutAsJsonAsync(
            $"/lsessions/{sessionId}/tasks/{taskId}",
            TestDataBuilder.AssignTaskRequest(),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, assigned.StatusCode);
        Assert.Contains(taskId, (await GetSessionAsync(sessionId)).AssignedTaskIds);

        HttpResponseMessage completed = await Client.PostAsJsonAsync(
            $"/tasks/{taskId}/complete",
            TestDataBuilder.CompleteTaskRequest(ownerId),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, completed.StatusCode);
        Assert.Equal("Completed", (await GetTaskAsync(taskId, ownerId)).Status);
        Assert.Equal(0, (await GetSessionAsync(sessionId)).Status);

        await EndSessionAsync(sessionId);
        Assert.Equal("Completed", (await GetTaskAsync(taskId, ownerId)).Status);
        Assert.Equal(1, (await GetSessionAsync(sessionId)).Status);
    }

    private async Task<Guid> CreatePlannedTaskAsync(Guid ownerId)
    {
        HttpResponseMessage drafted = await Client.PostAsJsonAsync(
            "/tasks/draft",
            TestDataBuilder.DraftTaskRequest(ownerId),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, drafted.StatusCode);
        Guid taskId = await drafted.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);

        HttpResponseMessage planned = await Client.PostAsJsonAsync(
            $"/tasks/{taskId}/plan",
            new { OwnerId = ownerId, PlannedAt = (DateTimeOffset?)null },
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, planned.StatusCode);
        return taskId;
    }

    private async Task<Guid> StartSessionAsync(Guid ownerId)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/lsessions",
            TestDataBuilder.StartSessionRequest(ownerId),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private async Task EndSessionAsync(Guid sessionId)
    {
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"/lsessions/{sessionId}/end",
            TestDataBuilder.EndSessionRequest(),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<TaskDetailResponse> GetTaskAsync(Guid taskId, Guid ownerId) =>
        await Client.GetFromJsonAsync<TaskDetailResponse>(
            $"/tasks/{taskId}?ownerId={ownerId}",
            TestContext.Current.CancellationToken)
        ?? throw new Xunit.Sdk.XunitException("The task response was empty.");

    private async Task<SessionResponse> GetSessionAsync(Guid sessionId) =>
        await Client.GetFromJsonAsync<SessionResponse>(
            $"/lsessions/{sessionId}",
            TestContext.Current.CancellationToken)
        ?? throw new Xunit.Sdk.XunitException("The Session response was empty.");
}
