using System.Net;
using System.Net.Http.Json;
using PCL_API.IntegrationTests.Infrastructure;
using PCL_API.IntegrationTests.TestData;
using Xunit;

namespace PCL_API.IntegrationTests.Evidence;

public sealed class EvidenceItemTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Add_and_list_should_return_note_and_link_evidence()
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid sessionId = await StartSessionAsync(ownerId);
        Guid noteId = await AddEvidenceAsync(sessionId, TestDataBuilder.AddNoteEvidenceRequest(ownerId, "Study notes"));
        Guid linkId = await AddEvidenceAsync(sessionId, TestDataBuilder.AddLinkEvidenceRequest(ownerId));

        IReadOnlyCollection<EvidenceItemResponse> items = await ListAsync(sessionId, ownerId);

        Assert.Equal(2, items.Count);
        Assert.Contains(items, item => item.Id == noteId && item.Type == "Note" && item.Content == "Study notes");
        Assert.Contains(items, item => item.Id == linkId && item.Type == "Link");
        Assert.All(items, item => Assert.Equal(ownerId, item.OwnerId));
    }

    [Fact]
    public async Task Remove_should_soft_remove_and_be_idempotent()
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid sessionId = await StartSessionAsync(ownerId);
        DateTimeOffset addedAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        Guid evidenceId = await AddEvidenceAsync(
            sessionId, TestDataBuilder.AddNoteEvidenceRequest(ownerId, addedAt: addedAt));

        HttpResponseMessage removed = await RemoveAsync(
            sessionId, evidenceId, TestDataBuilder.RemoveEvidenceRequest(ownerId, removalReason: "Superseded"));
        Assert.Equal(HttpStatusCode.NoContent, removed.StatusCode);
        Assert.Empty(await ListAsync(sessionId, ownerId));

        EvidenceItemResponse item = Assert.Single(await ListAsync(sessionId, ownerId, includeRemoved: true));
        Assert.NotNull(item.RemovedAt);
        Assert.Equal(ownerId, item.RemovedBy);
        Assert.Equal("Superseded", item.RemovalReason);

        HttpResponseMessage repeated = await RemoveAsync(
            sessionId, evidenceId, TestDataBuilder.RemoveEvidenceRequest(ownerId));
        Assert.Equal(HttpStatusCode.NoContent, repeated.StatusCode);
    }

    [Fact]
    public async Task Add_should_require_an_existing_active_session_owned_by_the_request_owner()
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        HttpResponseMessage missing = await Client.PostAsJsonAsync(
            $"/lsessions/{Guid.NewGuid()}/evidence-items", TestDataBuilder.AddNoteEvidenceRequest(ownerId),
            TestContext.Current.CancellationToken);
        await missing.ShouldBeProblemAsync(
            HttpStatusCode.NotFound, "SessionEvidenceAttachmentEligibility.SessionNotFound");

        Guid sessionId = await StartSessionAsync(ownerId);
        HttpResponseMessage wrongOwner = await Client.PostAsJsonAsync(
            $"/lsessions/{sessionId}/evidence-items",
            TestDataBuilder.AddNoteEvidenceRequest(TestDataBuilder.NewOwnerId()),
            TestContext.Current.CancellationToken);
        await wrongOwner.ShouldBeProblemAsync(
            HttpStatusCode.Conflict, "SessionEvidenceAttachmentEligibility.OwnerMismatch");

        await EndSessionAsync(sessionId);
        HttpResponseMessage stopped = await Client.PostAsJsonAsync(
            $"/lsessions/{sessionId}/evidence-items", TestDataBuilder.AddNoteEvidenceRequest(ownerId),
            TestContext.Current.CancellationToken);
        await stopped.ShouldBeProblemAsync(
            HttpStatusCode.Conflict, "SessionEvidenceAttachmentEligibility.NotActive");
    }

    [Theory]
    [InlineData(1, "")]
    [InlineData(2, "not-a-link")]
    [InlineData(99, "unsupported")]
    public async Task Add_should_reject_invalid_content_links_and_types(int type, string content)
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid sessionId = await StartSessionAsync(ownerId);
        var request = new { OwnerId = ownerId, Type = type, Content = content, AddedAt = DateTimeOffset.UtcNow };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/lsessions/{sessionId}/evidence-items", request, TestContext.Current.CancellationToken);

        if (type == 2)
        {
            await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "EvidenceItem.InvalidLink");
        }
        else
        {
            var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "General.Validation");
            problem.ShouldContainValidationErrors();
        }
    }

    [Theory]
    [InlineData(1, 10001)]
    [InlineData(2, 2049)]
    public async Task Add_should_reject_oversized_content(int type, int length)
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid sessionId = await StartSessionAsync(ownerId);
        var request = new { OwnerId = ownerId, Type = type, Content = new string('x', length), AddedAt = DateTimeOffset.UtcNow };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/lsessions/{sessionId}/evidence-items", request, TestContext.Current.CancellationToken);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "General.Validation");
        problem.ShouldContainValidationErrors();
    }

    [Fact]
    public async Task Generic_file_reference_should_report_the_upload_workflow_conflict()
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid sessionId = await StartSessionAsync(ownerId);
        var request = new { OwnerId = ownerId, Type = 3, Content = "caption", AddedAt = DateTimeOffset.UtcNow };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/lsessions/{sessionId}/evidence-items", request, TestContext.Current.CancellationToken);

        await response.ShouldBeProblemAsync(
            HttpStatusCode.Conflict, "EvidenceItem.FileReferenceRequiresUploadInitialization");
    }

    [Fact]
    public async Task Remove_should_validate_route_owner_time_and_reason()
    {
        Guid ownerId = TestDataBuilder.NewOwnerId();
        Guid sessionId = await StartSessionAsync(ownerId);
        DateTimeOffset addedAt = DateTimeOffset.UtcNow;
        Guid evidenceId = await AddEvidenceAsync(
            sessionId, TestDataBuilder.AddNoteEvidenceRequest(ownerId, addedAt: addedAt));

        HttpResponseMessage wrongSession = await RemoveAsync(
            Guid.NewGuid(), evidenceId, TestDataBuilder.RemoveEvidenceRequest(ownerId));
        await wrongSession.ShouldBeProblemAsync(HttpStatusCode.NotFound, "EvidenceItem.NotFound");

        HttpResponseMessage wrongOwner = await RemoveAsync(
            sessionId, evidenceId, TestDataBuilder.RemoveEvidenceRequest(TestDataBuilder.NewOwnerId()));
        await wrongOwner.ShouldBeProblemAsync(HttpStatusCode.Conflict, "EvidenceItem.OwnerMismatch");

        HttpResponseMessage invalidTime = await RemoveAsync(
            sessionId, evidenceId, TestDataBuilder.RemoveEvidenceRequest(ownerId, addedAt.AddSeconds(-1)));
        await invalidTime.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "EvidenceItem.InvalidRemovalTime");

        HttpResponseMessage longReason = await RemoveAsync(
            sessionId, evidenceId,
            TestDataBuilder.RemoveEvidenceRequest(ownerId, removalReason: new string('r', 1001)));
        var problem = await longReason.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "General.Validation");
        problem.ShouldContainValidationErrors();
    }

    private async Task<Guid> StartSessionAsync(Guid ownerId)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/lsessions", TestDataBuilder.StartSessionRequest(ownerId), TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private async Task EndSessionAsync(Guid sessionId)
    {
        HttpResponseMessage response = await Client.PutAsJsonAsync(
            $"/lsessions/{sessionId}/end", TestDataBuilder.EndSessionRequest(),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<Guid> AddEvidenceAsync(Guid sessionId, object request)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/lsessions/{sessionId}/evidence-items", request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private async Task<IReadOnlyCollection<EvidenceItemResponse>> ListAsync(
        Guid sessionId, Guid ownerId, bool includeRemoved = false) =>
        await Client.GetFromJsonAsync<IReadOnlyCollection<EvidenceItemResponse>>(
            $"/lsessions/{sessionId}/evidence-items?ownerId={ownerId}&includeRemoved={includeRemoved}",
            TestContext.Current.CancellationToken)
        ?? throw new Xunit.Sdk.XunitException("The evidence list response was empty.");

    private Task<HttpResponseMessage> RemoveAsync(Guid sessionId, Guid evidenceId, object request)
    {
        var message = new HttpRequestMessage(
            HttpMethod.Delete, $"/lsessions/{sessionId}/evidence-items/{evidenceId}")
        {
            Content = JsonContent.Create(request)
        };
        return Client.SendAsync(message, TestContext.Current.CancellationToken);
    }
}
