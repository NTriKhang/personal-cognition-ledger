using System.Net;
using System.Net.Http.Json;
using PCL_API.IntegrationTests.Infrastructure;
using PCL_API.IntegrationTests.TestData;
using Xunit;

namespace PCL_API.IntegrationTests.TaskPlanning;

public sealed class TaskPlanningLifecycleTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Plan_activate_defer_and_complete_should_follow_the_task_lifecycle()
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid taskId = await DraftTaskAsync(ownerId);

        await PostNoContentAsync($"/tasks/{taskId}/plan", new { OwnerId = ownerId, PlannedAt = (DateTimeOffset?)null });
        Assert.Equal("Planned", (await GetTaskAsync(taskId, ownerId)).Status);

        await PostNoContentAsync($"/tasks/{taskId}/activate", new { OwnerId = ownerId, ActivatedAt = (DateTimeOffset?)null });
        Assert.Equal("Active", (await GetTaskAsync(taskId, ownerId)).Status);

        await PostNoContentAsync($"/tasks/{taskId}/defer", new { OwnerId = ownerId, DeferredAt = (DateTimeOffset?)null });
        Assert.Equal("Planned", (await GetTaskAsync(taskId, ownerId)).Status);

        await PostNoContentAsync(
            $"/tasks/{taskId}/complete",
            TestDataBuilder.CompleteTaskRequest(ownerId, completionNote: "Lifecycle verified."));
        TaskDetailResponse completed = await GetTaskAsync(taskId, ownerId);
        Assert.Equal("Completed", completed.Status);
        Assert.Equal("Lifecycle verified.", completed.CompletionNote);
        Assert.NotNull(completed.CompletedAt);
    }

    [Fact]
    public async Task Cancel_should_cancel_a_draft_task()
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid taskId = await DraftTaskAsync(ownerId);

        await PostNoContentAsync(
            $"/tasks/{taskId}/cancel",
            TestDataBuilder.CancelTaskRequest(ownerId, cancellationReason: "No longer relevant."));

        TaskDetailResponse task = await GetTaskAsync(taskId, ownerId);
        Assert.Equal("Cancelled", task.Status);
        Assert.Equal("No longer relevant.", task.CancellationReason);
        Assert.NotNull(task.CancelledAt);
    }

    [Theory]
    [InlineData("activate", "Task.CannotActivate")]
    [InlineData("defer", "Task.CannotDefer")]
    [InlineData("complete", "Task.CannotComplete")]
    public async Task Lifecycle_should_reject_transitions_from_an_invalid_state(
        string operation,
        string expectedCode)
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid taskId = await DraftTaskAsync(ownerId);
        object request = operation == "complete"
            ? TestDataBuilder.CompleteTaskRequest(ownerId)
            : TransitionRequest(operation, ownerId);

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/tasks/{taskId}/{operation}", request, TestContext.Current.CancellationToken);

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, expectedCode);
    }

    [Fact]
    public async Task Planning_an_already_planned_task_should_conflict()
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid taskId = await DraftTaskAsync(ownerId);
        await PostNoContentAsync($"/tasks/{taskId}/plan", TransitionRequest("plan", ownerId));

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/tasks/{taskId}/plan", TransitionRequest("plan", ownerId), TestContext.Current.CancellationToken);

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, "Task.CannotPlan");
    }

    [Fact]
    public async Task Active_task_cancellation_should_require_a_reason()
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid taskId = await DraftTaskAsync(ownerId);
        await PostNoContentAsync($"/tasks/{taskId}/plan", TransitionRequest("plan", ownerId));
        await PostNoContentAsync($"/tasks/{taskId}/activate", TransitionRequest("activate", ownerId));

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/tasks/{taskId}/cancel",
            TestDataBuilder.CancelTaskRequest(ownerId),
            TestContext.Current.CancellationToken);

        await response.ShouldBeProblemAsync(
            HttpStatusCode.BadRequest,
            "Task.CancellationReasonRequired",
            "Cancellation reason is required when cancelling an active task.");
    }

    [Theory]
    [InlineData("complete", "Task.InvalidCompletionTime")]
    [InlineData("cancel", "Task.InvalidCancellationTime")]
    public async Task Terminal_transition_should_reject_a_timestamp_before_creation(
        string operation,
        string expectedCode)
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        DateTimeOffset createdAt = DateTimeOffset.UtcNow;
        Guid taskId = await DraftTaskAsync(ownerId, createdAt);
        object request;
        if (operation == "complete")
        {
            await PostNoContentAsync($"/tasks/{taskId}/plan", TransitionRequest("plan", ownerId));
            request = TestDataBuilder.CompleteTaskRequest(ownerId, createdAt.AddMinutes(-1));
        }
        else
        {
            request = TestDataBuilder.CancelTaskRequest(ownerId, createdAt.AddMinutes(-1));
        }

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/tasks/{taskId}/{operation}", request, TestContext.Current.CancellationToken);

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, expectedCode);
    }

    [Theory]
    [InlineData("complete")]
    [InlineData("cancel")]
    public async Task Terminal_tasks_should_reject_further_mutation(string terminalOperation)
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid taskId = await DraftTaskAsync(ownerId);
        if (terminalOperation == "complete")
        {
            await PostNoContentAsync($"/tasks/{taskId}/plan", TransitionRequest("plan", ownerId));
            await PostNoContentAsync($"/tasks/{taskId}/complete", TestDataBuilder.CompleteTaskRequest(ownerId));
        }
        else
        {
            await PostNoContentAsync($"/tasks/{taskId}/cancel", TestDataBuilder.CancelTaskRequest(ownerId));
        }

        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"/tasks/{taskId}/details",
            TestDataBuilder.RefineTaskRequest(ownerId),
            TestContext.Current.CancellationToken);

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, "Task.AlreadyTerminal");
    }

    [Theory]
    [InlineData("plan")]
    [InlineData("activate")]
    [InlineData("defer")]
    [InlineData("complete")]
    [InlineData("cancel")]
    public async Task Lifecycle_should_hide_owner_mismatch_as_not_found(string operation)
    {
        Guid taskId = await DraftTaskAsync(TestDataBuilder.NewOwnerId());
        Guid wrongOwnerId = TestDataBuilder.NewOwnerId();
        object request = operation switch
        {
            "complete" => TestDataBuilder.CompleteTaskRequest(wrongOwnerId),
            "cancel" => TestDataBuilder.CancelTaskRequest(wrongOwnerId),
            _ => TransitionRequest(operation, wrongOwnerId)
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/tasks/{taskId}/{operation}", request, TestContext.Current.CancellationToken);

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, "Task.NotFound");
    }

    [Fact]
    public async Task Lifecycle_should_report_a_missing_task()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/tasks/{Guid.NewGuid()}/plan",
            TransitionRequest("plan", TestDataBuilder.NewOwnerId()),
            TestContext.Current.CancellationToken);

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, "Task.NotFound");
    }

    [Theory]
    [InlineData("complete")]
    [InlineData("cancel")]
    public async Task Terminal_notes_should_enforce_length_validation(string operation)
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid taskId = await DraftTaskAsync(ownerId);
        object request = operation == "complete"
            ? TestDataBuilder.CompleteTaskRequest(ownerId, completionNote: new string('N', 2001))
            : TestDataBuilder.CancelTaskRequest(ownerId, cancellationReason: new string('R', 2001));

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/tasks/{taskId}/{operation}", request, TestContext.Current.CancellationToken);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "General.Validation");
        problem.ShouldContainValidationErrors();
    }

    private async Task<Guid> DraftTaskAsync(Guid ownerId, DateTimeOffset? createdAt = null)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/tasks/draft",
            TestDataBuilder.DraftTaskRequest(ownerId, createdAt: createdAt),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private async Task<TaskDetailResponse> GetTaskAsync(Guid taskId, Guid ownerId)
    {
        TaskDetailResponse? task = await Client.GetFromJsonAsync<TaskDetailResponse>(
            $"/tasks/{taskId}?ownerId={ownerId}", TestContext.Current.CancellationToken);
        return task ?? throw new Xunit.Sdk.XunitException("The task detail response was empty.");
    }

    private async Task PostNoContentAsync(string uri, object request)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            uri, request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private static object TransitionRequest(string operation, Guid ownerId) => operation switch
    {
        "plan" => new { OwnerId = ownerId, PlannedAt = (DateTimeOffset?)null },
        "activate" => new { OwnerId = ownerId, ActivatedAt = (DateTimeOffset?)null },
        "defer" => new { OwnerId = ownerId, DeferredAt = (DateTimeOffset?)null },
        _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
    };
}
