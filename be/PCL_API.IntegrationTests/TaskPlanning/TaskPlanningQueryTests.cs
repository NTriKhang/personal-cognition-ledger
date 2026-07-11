using System.Net;
using System.Net.Http.Json;
using PCL_API.IntegrationTests.Infrastructure;
using PCL_API.IntegrationTests.TestData;
using Xunit;

namespace PCL_API.IntegrationTests.TaskPlanning;

public sealed class TaskPlanningQueryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Draft_and_get_should_return_the_created_task()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Guid ownerId = TestDataBuilder.NewOwnerId();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow.AddMinutes(-5);

        Guid taskId = await DraftTaskAsync(ownerId, "Read architecture notes", createdAt);
        HttpResponseMessage response = await Client.GetAsync(
            $"/tasks/{taskId}?ownerId={ownerId}", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        TaskDetailResponse? task = await response.Content.ReadFromJsonAsync<TaskDetailResponse>(cancellationToken);
        Assert.NotNull(task);
        Assert.Equal(taskId, task.Id);
        Assert.Equal(ownerId, task.OwnerId);
        Assert.Equal("Read architecture notes", task.Title);
        Assert.Equal("Draft", task.Status);
        Assert.Equal("Medium", task.Priority);
        Assert.InRange((task.CreatedAt - createdAt).Duration(), TimeSpan.Zero, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task Get_should_hide_a_task_from_another_owner_and_report_missing_tasks()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Guid taskId = await DraftTaskAsync(TestDataBuilder.NewOwnerId());

        HttpResponseMessage wrongOwner = await Client.GetAsync(
            $"/tasks/{taskId}?ownerId={TestDataBuilder.NewOwnerId()}", cancellationToken);
        await wrongOwner.ShouldBeProblemAsync(HttpStatusCode.NotFound, "Task.NotFound");

        Guid missingTaskId = Guid.NewGuid();
        HttpResponseMessage missing = await Client.GetAsync(
            $"/tasks/{missingTaskId}?ownerId={TestDataBuilder.NewOwnerId()}", cancellationToken);
        await missing.ShouldBeProblemAsync(HttpStatusCode.NotFound, "Task.NotFound");
    }

    [Fact]
    public async Task List_should_return_only_the_requested_owners_tasks()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid firstId = await DraftTaskAsync(ownerId, "First task");
        Guid secondId = await DraftTaskAsync(ownerId, "Second task");
        await DraftTaskAsync(TestDataBuilder.NewOwnerId(), "Another owner's task");

        IReadOnlyCollection<TaskSummaryResponse> tasks = await GetTasksAsync(
            $"/tasks?ownerId={ownerId}", cancellationToken);

        Assert.Equal(2, tasks.Count);
        Assert.Contains(tasks, task => task.Id == firstId);
        Assert.Contains(tasks, task => task.Id == secondId);
        Assert.All(tasks, task => Assert.Equal(ownerId, task.OwnerId));
    }

    [Fact]
    public async Task List_should_apply_status_category_priority_search_and_creation_filters()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Guid ownerId = TestDataBuilder.NewOwnerId();
        DateTimeOffset includedAt = DateTimeOffset.UtcNow.AddHours(-1);
        Guid includedId = await DraftTaskAsync(ownerId, "Focused coding practice", includedAt);
        await PutAsync($"/tasks/{includedId}/category", TestDataBuilder.CategorizeTaskRequest(ownerId), cancellationToken);
        await PutAsync($"/tasks/{includedId}/priority", TestDataBuilder.PrioritizeTaskRequest(ownerId), cancellationToken);
        await PostAsync($"/tasks/{includedId}/plan", new { OwnerId = ownerId, PlannedAt = (DateTimeOffset?)null }, cancellationToken);

        await DraftTaskAsync(ownerId, "Unrelated old task", DateTimeOffset.UtcNow.AddDays(-10));
        await DraftTaskAsync(ownerId, "Unrelated recent task", DateTimeOffset.UtcNow.AddMinutes(-10));

        string from = Uri.EscapeDataString(includedAt.AddMinutes(-1).ToString("O"));
        string to = Uri.EscapeDataString(includedAt.AddMinutes(1).ToString("O"));
        IReadOnlyCollection<TaskSummaryResponse> tasks = await GetTasksAsync(
            $"/tasks?ownerId={ownerId}&status=Planned&category=Coding&priority=High&search=coding&createdFrom={from}&createdTo={to}",
            cancellationToken);

        TaskSummaryResponse task = Assert.Single(tasks);
        Assert.Equal(includedId, task.Id);
        Assert.Equal("Planned", task.Status);
        Assert.Equal("Coding", task.Category);
        Assert.Equal("High", task.Priority);
    }

    [Fact]
    public async Task Assignable_list_should_include_only_planned_and_active_tasks()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Guid ownerId = TestDataBuilder.NewOwnerId();
        await DraftTaskAsync(ownerId, "Draft task");
        Guid plannedId = await DraftTaskAsync(ownerId, "Planned task");
        await PostAsync($"/tasks/{plannedId}/plan", new { OwnerId = ownerId, PlannedAt = (DateTimeOffset?)null }, cancellationToken);
        Guid activeId = await DraftTaskAsync(ownerId, "Active task");
        await PostAsync($"/tasks/{activeId}/plan", new { OwnerId = ownerId, PlannedAt = (DateTimeOffset?)null }, cancellationToken);
        await PostAsync($"/tasks/{activeId}/activate", new { OwnerId = ownerId, ActivatedAt = (DateTimeOffset?)null }, cancellationToken);

        IReadOnlyCollection<TaskSummaryResponse> tasks = await GetTasksAsync(
            $"/tasks/assignable?ownerId={ownerId}", cancellationToken);

        Assert.Equal(2, tasks.Count);
        Assert.Contains(tasks, task => task.Id == plannedId);
        Assert.Contains(tasks, task => task.Id == activeId);
        Assert.All(tasks, task => Assert.Contains(task.Status, new[] { "Planned", "Active" }));
    }

    [Theory]
    [InlineData("status=Unknown")]
    [InlineData("category=Unknown")]
    [InlineData("priority=Unknown")]
    public async Task List_should_reject_invalid_enum_filters(string filter)
    {
        HttpResponseMessage response = await Client.GetAsync(
            $"/tasks?ownerId={TestDataBuilder.NewOwnerId()}&{filter}",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<Guid> DraftTaskAsync(
        Guid ownerId,
        string title = "Integration test task",
        DateTimeOffset? createdAt = null)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/tasks/draft",
            TestDataBuilder.DraftTaskRequest(ownerId, title, createdAt: createdAt),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private async Task<IReadOnlyCollection<TaskSummaryResponse>> GetTasksAsync(
        string uri,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await Client.GetAsync(uri, cancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<TaskSummaryResponse>>(cancellationToken)
            ?? throw new Xunit.Sdk.XunitException("The task list response was empty.");
    }

    private async Task PutAsync(string uri, object request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await Client.PutAsJsonAsync(uri, request, cancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task PostAsync(string uri, object request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(uri, request, cancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
