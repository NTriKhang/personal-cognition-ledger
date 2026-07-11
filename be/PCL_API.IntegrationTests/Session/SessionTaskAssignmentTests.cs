using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PCL.Modules.Session.Domain.LSessions;
using PCL.Modules.Session.Infrastructure.Database;
using PCL_API.IntegrationTests.Infrastructure;
using PCL_API.IntegrationTests.TestData;
using Xunit;

namespace PCL_API.IntegrationTests.Session;

public sealed class SessionTaskAssignmentTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Assign_and_remove_should_support_eligible_tasks(bool activateTask)
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid sessionId = await StartSessionAsync(ownerId);
        Guid taskId = await CreatePlannedTaskAsync(ownerId);
        if (activateTask)
        {
            await PostNoContentAsync($"/tasks/{taskId}/activate", new { OwnerId = ownerId, ActivatedAt = (DateTimeOffset?)null });
        }

        HttpResponseMessage assigned = await Client.PutAsJsonAsync(
            $"/lsessions/{sessionId}/tasks/{taskId}", TestDataBuilder.AssignTaskRequest(),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, assigned.StatusCode);
        Assert.Contains(taskId, (await GetSessionAsync(sessionId)).AssignedTaskIds);

        HttpResponseMessage duplicate = await Client.PutAsJsonAsync(
            $"/lsessions/{sessionId}/tasks/{taskId}", TestDataBuilder.AssignTaskRequest(),
            TestContext.Current.CancellationToken);
        await duplicate.ShouldBeProblemAsync(HttpStatusCode.Conflict, "LSession.TaskAlreadyAssigned");

        HttpResponseMessage removed = await Client.DeleteAsync(
            $"/lsessions/{sessionId}/tasks/{taskId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, removed.StatusCode);
        Assert.DoesNotContain(taskId, (await GetSessionAsync(sessionId)).AssignedTaskIds);

        HttpResponseMessage repeated = await Client.DeleteAsync(
            $"/lsessions/{sessionId}/tasks/{taskId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, repeated.StatusCode);
    }

    [Fact]
    public async Task Assign_should_reject_ineligible_missing_and_wrong_owner_tasks()
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid sessionId = await StartSessionAsync(ownerId);
        Guid draftTaskId = await DraftTaskAsync(ownerId);

        HttpResponseMessage ineligible = await AssignAsync(sessionId, draftTaskId);
        await ineligible.ShouldBeProblemAsync(HttpStatusCode.Conflict, "TaskAssignmentEligibility.NotAssignable");

        HttpResponseMessage missing = await AssignAsync(sessionId, Guid.NewGuid());
        await missing.ShouldBeProblemAsync(HttpStatusCode.NotFound, "TaskAssignmentEligibility.TaskNotFound");

        Guid otherOwnersTask = await CreatePlannedTaskAsync(TestDataBuilder.NewOwnerId());
        HttpResponseMessage wrongOwner = await AssignAsync(sessionId, otherOwnersTask);
        await wrongOwner.ShouldBeProblemAsync(HttpStatusCode.Conflict, "TaskAssignmentEligibility.OwnerMismatch");
    }

    [Fact]
    public async Task Assignment_should_report_missing_session()
    {
        HttpResponseMessage assign = await AssignAsync(Guid.NewGuid(), Guid.NewGuid());
        await assign.ShouldBeProblemAsync(HttpStatusCode.NotFound, "LSession.NotFound");

        HttpResponseMessage remove = await Client.DeleteAsync(
            $"/lsessions/{Guid.NewGuid()}/tasks/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        await remove.ShouldBeProblemAsync(HttpStatusCode.NotFound, "LSession.NotFound");
    }

    [Fact]
    public async Task Stopped_session_should_reject_assignment_mutations()
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid sessionId = await StartSessionAsync(ownerId);
        Guid taskId = await CreatePlannedTaskAsync(ownerId);
        Assert.Equal(HttpStatusCode.OK, (await AssignAsync(sessionId, taskId)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Client.PutAsJsonAsync(
            $"/lsessions/{sessionId}/end", TestDataBuilder.EndSessionRequest(),
            TestContext.Current.CancellationToken)).StatusCode);

        Guid anotherTaskId = await CreatePlannedTaskAsync(ownerId);
        HttpResponseMessage assign = await AssignAsync(sessionId, anotherTaskId);
        await assign.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "LSession.NotActive");

        HttpResponseMessage remove = await Client.DeleteAsync(
            $"/lsessions/{sessionId}/tasks/{taskId}", TestContext.Current.CancellationToken);
        await remove.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "LSession.NotActive");
    }

    [Fact]
    public async Task Task_assigned_to_another_active_session_should_conflict()
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        (Guid firstSessionId, Guid secondSessionId) = await SeedTwoActiveSessionsAsync(ownerId);
        Guid taskId = await CreatePlannedTaskAsync(ownerId);
        Assert.Equal(HttpStatusCode.OK, (await AssignAsync(firstSessionId, taskId)).StatusCode);

        HttpResponseMessage response = await AssignAsync(secondSessionId, taskId);

        await response.ShouldBeProblemAsync(
            HttpStatusCode.Conflict, "LSession.TaskAlreadyAssignedToAnotherActiveSession");
    }

    private async Task<(Guid First, Guid Second)> SeedTwoActiveSessionsAsync(Guid ownerId)
    {
        LSession first = LSession.StartNew(ownerId, "Seeded first session", DateTimeOffset.UtcNow).Value;
        LSession second = LSession.StartNew(ownerId, "Seeded second session", DateTimeOffset.UtcNow).Value;
        using IServiceScope scope = Fixture.Factory.Services.CreateScope();
        LSessionDbContext dbContext = scope.ServiceProvider.GetRequiredService<LSessionDbContext>();
        dbContext.LSessions.AddRange(first, second);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return (first.Id, second.Id);
    }

    private async Task<Guid> StartSessionAsync(Guid ownerId)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/lsessions", TestDataBuilder.StartSessionRequest(ownerId), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private async Task<Guid> DraftTaskAsync(Guid ownerId)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/tasks/draft", TestDataBuilder.DraftTaskRequest(ownerId), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private async Task<Guid> CreatePlannedTaskAsync(Guid ownerId)
    {
        Guid taskId = await DraftTaskAsync(ownerId);
        await PostNoContentAsync($"/tasks/{taskId}/plan", new { OwnerId = ownerId, PlannedAt = (DateTimeOffset?)null });
        return taskId;
    }

    private async Task PostNoContentAsync(string uri, object request)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(uri, request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private Task<HttpResponseMessage> AssignAsync(Guid sessionId, Guid taskId) =>
        Client.PutAsJsonAsync(
            $"/lsessions/{sessionId}/tasks/{taskId}", TestDataBuilder.AssignTaskRequest(),
            TestContext.Current.CancellationToken);

    private async Task<SessionResponse> GetSessionAsync(Guid sessionId) =>
        await Client.GetFromJsonAsync<SessionResponse>(
            $"/lsessions/{sessionId}", TestContext.Current.CancellationToken)
        ?? throw new Xunit.Sdk.XunitException("The session response was empty.");
}
