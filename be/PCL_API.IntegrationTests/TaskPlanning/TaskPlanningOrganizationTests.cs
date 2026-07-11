using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using PCL_API.IntegrationTests.Infrastructure;
using PCL_API.IntegrationTests.TestData;
using Xunit;

namespace PCL_API.IntegrationTests.TaskPlanning;

public sealed class TaskPlanningOrganizationTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Refine_categorize_and_prioritize_should_update_task_details()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid taskId = await DraftTaskAsync(ownerId);

        await ShouldBeNoContentAsync(Client.PutAsJsonAsync(
            $"/tasks/{taskId}/details",
            TestDataBuilder.RefineTaskRequest(ownerId, "Refined title", "Refined description"),
            cancellationToken));
        await ShouldBeNoContentAsync(Client.PutAsJsonAsync(
            $"/tasks/{taskId}/category",
            TestDataBuilder.CategorizeTaskRequest(ownerId, category: 5),
            cancellationToken));
        await ShouldBeNoContentAsync(Client.PutAsJsonAsync(
            $"/tasks/{taskId}/priority",
            TestDataBuilder.PrioritizeTaskRequest(ownerId, priority: 2),
            cancellationToken));

        TaskDetailResponse task = await GetTaskAsync(taskId, ownerId);
        Assert.Equal("Refined title", task.Title);
        Assert.Equal("Refined description", task.Description);
        Assert.Equal("Research", task.Category);
        Assert.Equal("High", task.Priority);
        Assert.NotNull(task.UpdatedAt);
    }

    [Theory]
    [InlineData("category")]
    [InlineData("priority")]
    public async Task Organization_should_reject_invalid_enum_values(string operation)
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid taskId = await DraftTaskAsync(ownerId);
        object request = operation == "category"
            ? TestDataBuilder.CategorizeTaskRequest(ownerId, 999)
            : TestDataBuilder.PrioritizeTaskRequest(ownerId, 999);

        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"/tasks/{taskId}/{operation}", request, TestContext.Current.CancellationToken);

        ProblemDetails problem = await response.ShouldBeProblemAsync(
            HttpStatusCode.BadRequest,
            "General.Validation",
            "One or more validation errors occurred");
        problem.ShouldContainValidationErrors();
    }

    [Theory]
    [InlineData(201, 0)]
    [InlineData(10, 4001)]
    public async Task Refine_should_reject_oversized_text(int titleLength, int descriptionLength)
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid taskId = await DraftTaskAsync(ownerId);

        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"/tasks/{taskId}/details",
            TestDataBuilder.RefineTaskRequest(
                ownerId,
                new string('T', titleLength),
                new string('D', descriptionLength)),
            TestContext.Current.CancellationToken);

        ProblemDetails problem = await response.ShouldBeProblemAsync(
            HttpStatusCode.BadRequest, "General.Validation");
        problem.ShouldContainValidationErrors();
    }

    [Fact]
    public async Task Draft_should_reject_missing_owner_and_oversized_content()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/tasks/draft",
            TestDataBuilder.DraftTaskRequest(
                Guid.Empty,
                new string('T', 201),
                new string('D', 4001)),
            TestContext.Current.CancellationToken);

        ProblemDetails problem = await response.ShouldBeProblemAsync(
            HttpStatusCode.BadRequest, "General.Validation");
        problem.ShouldContainValidationErrors();
    }

    [Theory]
    [InlineData("details")]
    [InlineData("category")]
    [InlineData("priority")]
    public async Task Organization_should_hide_owner_mismatch_as_not_found(string operation)
    {
        Guid taskId = await DraftTaskAsync(TestDataBuilder.NewOwnerId());
        Guid wrongOwnerId = TestDataBuilder.NewOwnerId();
        object request = operation switch
        {
            "details" => TestDataBuilder.RefineTaskRequest(wrongOwnerId),
            "category" => TestDataBuilder.CategorizeTaskRequest(wrongOwnerId),
            _ => TestDataBuilder.PrioritizeTaskRequest(wrongOwnerId)
        };

        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"/tasks/{taskId}/{operation}", request, TestContext.Current.CancellationToken);

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, "Task.NotFound");
    }

    private async Task<Guid> DraftTaskAsync(Guid ownerId)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/tasks/draft", TestDataBuilder.DraftTaskRequest(ownerId), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private async Task<TaskDetailResponse> GetTaskAsync(Guid taskId, Guid ownerId)
    {
        TaskDetailResponse? task = await Client.GetFromJsonAsync<TaskDetailResponse>(
            $"/tasks/{taskId}?ownerId={ownerId}", TestContext.Current.CancellationToken);
        return task ?? throw new Xunit.Sdk.XunitException("The task detail response was empty.");
    }

    private static async Task ShouldBeNoContentAsync(Task<HttpResponseMessage> responseTask)
    {
        using HttpResponseMessage response = await responseTask;
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
