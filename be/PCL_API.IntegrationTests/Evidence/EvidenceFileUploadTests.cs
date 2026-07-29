using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using PCL.Modules.Evidence.Domain.EvidenceItems;
using PCL.Modules.Evidence.Infrastructure.Database;
using Quartz;
using PCL_API.IntegrationTests.Infrastructure;
using PCL_API.IntegrationTests.TestData;
using Xunit;

namespace PCL_API.IntegrationTests.Evidence;

public sealed class EvidenceFileUploadTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Local_file_should_initialize_upload_confirm_and_download()
    {
        Guid owner = TestDataBuilder.NewOwnerId();
        Guid session = await StartSession(owner);
        string root = Path.Combine(Fixture.LocalStorageRoot, Guid.NewGuid().ToString("N"));
        HttpResponseMessage createdProfile = await Client.PostAsJsonAsync(
            "/admin/evidence-storage/profiles/local",
            TestDataBuilder.CreateLocalStorageProfileRequest(root),
            TestContext.Current.CancellationToken
        );
        Guid profile = await createdProfile.Content.ReadFromJsonAsync<Guid>(
            TestContext.Current.CancellationToken
        );
        await Client.PutAsJsonAsync(
            "/admin/evidence-storage/settings/active-profile",
            TestDataBuilder.SelectActiveStorageProfileRequest(profile),
            TestContext.Current.CancellationToken
        );
        byte[] content = "evidence file"u8.ToArray();
        string checksum = Convert.ToBase64String(SHA256.HashData(content));
        HttpResponseMessage initialized = await Initialize(session,
            new
            {
                OwnerId = owner,
                Caption = "receipt",
                OriginalFileName = "receipt.txt",
                ContentType = "text/plain",
                FileSizeBytes = content.LongLength,
                ChecksumAlgorithm = "SHA256",
                ChecksumValue = checksum,
            });
        Assert.Equal(HttpStatusCode.Created, initialized.StatusCode);
        FileUploadResponse upload = (
            await initialized.Content.ReadFromJsonAsync<FileUploadResponse>(
                TestContext.Current.CancellationToken
            )
        )!;
        Assert.Equal("ApiProxy", upload.UploadMode);
        Assert.Equal("Pending", upload.Status);
        using var bytes = new ByteArrayContent(content);
        bytes.Headers.ContentType = new("text/plain");
        HttpResponseMessage transferred = await Client.PutAsync(
            upload.UploadUrl,
            bytes,
            TestContext.Current.CancellationToken
        );
        Assert.Equal(HttpStatusCode.NoContent, transferred.StatusCode);
        HttpResponseMessage confirmed = await Client.PostAsJsonAsync(
            $"/lsessions/{session}/evidence-items/{upload.EvidenceItemId}/file-upload/confirm",
            new { OwnerId = owner, UploadAttemptId = upload.UploadAttemptId },
            TestContext.Current.CancellationToken
        );
        Assert.Equal(HttpStatusCode.OK, confirmed.StatusCode);
        FileUploadResponse ready = (
            await confirmed.Content.ReadFromJsonAsync<FileUploadResponse>(
                TestContext.Current.CancellationToken
            )
        )!;
        Assert.Equal("Ready", ready.Status);
        byte[] downloaded = await Client.GetByteArrayAsync(
            $"/lsessions/{session}/evidence-items/{upload.EvidenceItemId}/file?ownerId={owner}",
            TestContext.Current.CancellationToken
        );
        Assert.Equal(content, downloaded);
    }

    [Fact]
    public async Task Initialization_should_require_an_active_storage_profile()
    {
        Guid owner = TestDataBuilder.NewOwnerId();
        Guid session = await StartSession(owner);
        byte[] data = "x"u8.ToArray();
        HttpResponseMessage response = await Initialize(session,
            new
            {
                OwnerId = owner,
                Caption = (string?)null,
                OriginalFileName = "x.txt",
                ContentType = "text/plain",
                FileSizeBytes = 1,
                ChecksumAlgorithm = "SHA256",
                ChecksumValue = Convert.ToBase64String(SHA256.HashData(data)),
            });
        await response.ShouldBeProblemAsync(
            HttpStatusCode.Conflict,
            "EvidenceFile.StorageNotConfigured"
        );
    }

    [Fact]
    public async Task Initialization_retry_should_return_the_same_reservation()
    {
        Guid owner = TestDataBuilder.NewOwnerId();
        Guid session = await StartSession(owner);
        await CreateLocalProfile();
        byte[] data = "same"u8.ToArray();
        var body = new { OwnerId = owner, Caption = "retry", OriginalFileName = "same.txt", ContentType = "text/plain",
            FileSizeBytes = data.LongLength, ChecksumAlgorithm = "SHA256", ChecksumValue = Convert.ToBase64String(SHA256.HashData(data)) };
        string key = Guid.NewGuid().ToString("N");
        HttpResponseMessage first = await Initialize(session, body, key);
        HttpResponseMessage replay = await Initialize(session, body, key);
        FileUploadResponse a = (await first.Content.ReadFromJsonAsync<FileUploadResponse>(TestContext.Current.CancellationToken))!;
        FileUploadResponse b = (await replay.Content.ReadFromJsonAsync<FileUploadResponse>(TestContext.Current.CancellationToken))!;
        Assert.Equal(a.EvidenceItemId, b.EvidenceItemId);
        Assert.Equal(a.UploadAttemptId, b.UploadAttemptId);
    }

    [Fact]
    public async Task Expired_upload_should_renew_with_a_fresh_attempt()
    {
        Guid owner = TestDataBuilder.NewOwnerId();
        Guid session = await StartSession(owner);
        await CreateLocalProfile();
        byte[] data = "renew"u8.ToArray();
        HttpResponseMessage initialized = await Initialize(session, new { OwnerId = owner, Caption = "renew", OriginalFileName = "renew.txt",
            ContentType = "text/plain", FileSizeBytes = data.LongLength, ChecksumAlgorithm = "SHA256",
            ChecksumValue = Convert.ToBase64String(SHA256.HashData(data)) });
        FileUploadResponse upload = (await initialized.Content.ReadFromJsonAsync<FileUploadResponse>(TestContext.Current.CancellationToken))!;
        using (IServiceScope scope = Fixture.Factory.Services.CreateScope())
        {
            EvidenceDbContext db = scope.ServiceProvider.GetRequiredService<EvidenceDbContext>();
            await db.EvidenceFiles.Where(x => x.EvidenceItemId == EvidenceItemId.From(upload.EvidenceItemId))
                .ExecuteUpdateAsync(x => x.SetProperty(f => f.UploadExpiresAt, DateTimeOffset.UtcNow.AddMinutes(-1)), TestContext.Current.CancellationToken);
        }
        await Client.PostAsJsonAsync($"/lsessions/{session}/evidence-items/{upload.EvidenceItemId}/file-upload/confirm",
            new { OwnerId = owner, UploadAttemptId = upload.UploadAttemptId }, TestContext.Current.CancellationToken);
        HttpResponseMessage renewed = await Client.PostAsJsonAsync($"/lsessions/{session}/evidence-items/{upload.EvidenceItemId}/file-upload/renew",
            new { OwnerId = owner, UploadAttemptId = upload.UploadAttemptId }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, renewed.StatusCode);
        FileUploadResponse next = (await renewed.Content.ReadFromJsonAsync<FileUploadResponse>(TestContext.Current.CancellationToken))!;
        Assert.NotEqual(upload.UploadAttemptId, next.UploadAttemptId);
        Assert.Equal("Pending", next.Status);
    }

    [Fact]
    public async Task Pending_upload_should_cancel_idempotently()
    {
        Guid owner = TestDataBuilder.NewOwnerId();
        Guid session = await StartSession(owner);
        await CreateLocalProfile();
        byte[] data = "cancel"u8.ToArray();
        HttpResponseMessage initialized = await Initialize(session, new { OwnerId = owner, Caption = "cancel", OriginalFileName = "cancel.txt",
            ContentType = "text/plain", FileSizeBytes = data.LongLength, ChecksumAlgorithm = "SHA256",
            ChecksumValue = Convert.ToBase64String(SHA256.HashData(data)) });
        FileUploadResponse upload = (await initialized.Content.ReadFromJsonAsync<FileUploadResponse>(TestContext.Current.CancellationToken))!;
        string route = $"/lsessions/{session}/evidence-items/{upload.EvidenceItemId}/file-upload?ownerId={owner}&uploadAttemptId={upload.UploadAttemptId}";
        HttpResponseMessage first = await Client.DeleteAsync(route, TestContext.Current.CancellationToken);
        HttpResponseMessage replay = await Client.DeleteAsync(route, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
        FileUploadResponse result = (await replay.Content.ReadFromJsonAsync<FileUploadResponse>(TestContext.Current.CancellationToken))!;
        Assert.Equal("Cancelled", result.Status);
    }

    [Fact]
    public async Task Reconciliation_should_mark_uploaded_content_ready()
    {
        Guid owner = TestDataBuilder.NewOwnerId();
        Guid session = await StartSession(owner);
        await CreateLocalProfile();
        byte[] data = "reconcile"u8.ToArray();
        HttpResponseMessage initialized = await Initialize(session, new { OwnerId = owner, Caption = "reconcile", OriginalFileName = "reconcile.txt",
            ContentType = "text/plain", FileSizeBytes = data.LongLength, ChecksumAlgorithm = "SHA256",
            ChecksumValue = Convert.ToBase64String(SHA256.HashData(data)) });
        FileUploadResponse upload = (await initialized.Content.ReadFromJsonAsync<FileUploadResponse>(TestContext.Current.CancellationToken))!;
        using var bytes = new ByteArrayContent(data);
        bytes.Headers.ContentType = new("text/plain");
        (await Client.PutAsync(upload.UploadUrl, bytes, TestContext.Current.CancellationToken)).EnsureSuccessStatusCode();
        await UpdateFile(upload.EvidenceItemId, x => x.SetProperty(f => f.UploadExpiresAt, DateTimeOffset.UtcNow));
        await Trigger("Evidence.FileUploadReconciliation");
        await WaitForFile(upload.EvidenceItemId, x => x.UploadStatus == EvidenceFileUploadStatus.Ready);
    }

    [Fact]
    public async Task Cleanup_should_process_cancelled_attempt_after_retention()
    {
        Guid owner = TestDataBuilder.NewOwnerId();
        Guid session = await StartSession(owner);
        await CreateLocalProfile();
        byte[] data = "cleanup"u8.ToArray();
        HttpResponseMessage initialized = await Initialize(session, new { OwnerId = owner, Caption = "cleanup", OriginalFileName = "cleanup.txt",
            ContentType = "text/plain", FileSizeBytes = data.LongLength, ChecksumAlgorithm = "SHA256",
            ChecksumValue = Convert.ToBase64String(SHA256.HashData(data)) });
        FileUploadResponse upload = (await initialized.Content.ReadFromJsonAsync<FileUploadResponse>(TestContext.Current.CancellationToken))!;
        await Client.DeleteAsync($"/lsessions/{session}/evidence-items/{upload.EvidenceItemId}/file-upload?ownerId={owner}&uploadAttemptId={upload.UploadAttemptId}", TestContext.Current.CancellationToken);
        await UpdateFile(upload.EvidenceItemId, x => x.SetProperty(f => f.StatusChangedAt, DateTimeOffset.UtcNow.AddDays(-8)));
        await Trigger("Evidence.FileCleanup");
        await WaitForFile(upload.EvidenceItemId, x => x.PhysicalDeletedAt != null);
    }

    private async Task<Guid> StartSession(Guid owner)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/lsessions",
            TestDataBuilder.StartSessionRequest(owner),
            TestContext.Current.CancellationToken
        );
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>(
            TestContext.Current.CancellationToken
        );
    }

    private async Task<HttpResponseMessage> Initialize(Guid session, object body, string? key = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/lsessions/{session}/evidence-items/file-uploads")
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Add("Idempotency-Key", key ?? Guid.NewGuid().ToString("N"));
        return await Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private async Task CreateLocalProfile()
    {
        string root = Path.Combine(Fixture.LocalStorageRoot, Guid.NewGuid().ToString("N"));
        HttpResponseMessage created = await Client.PostAsJsonAsync("/admin/evidence-storage/profiles/local",
            TestDataBuilder.CreateLocalStorageProfileRequest(root), TestContext.Current.CancellationToken);
        Guid profile = await created.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
        await Client.PutAsJsonAsync("/admin/evidence-storage/settings/active-profile",
            TestDataBuilder.SelectActiveStorageProfileRequest(profile), TestContext.Current.CancellationToken);
    }

    private async Task UpdateFile(Guid itemId, System.Linq.Expressions.Expression<Func<SetPropertyCalls<EvidenceFile>, SetPropertyCalls<EvidenceFile>>> update)
    {
        using IServiceScope scope = Fixture.Factory.Services.CreateScope();
        EvidenceDbContext db = scope.ServiceProvider.GetRequiredService<EvidenceDbContext>();
        await db.EvidenceFiles.Where(x => x.EvidenceItemId == EvidenceItemId.From(itemId))
            .ExecuteUpdateAsync(update, TestContext.Current.CancellationToken);
    }

    private async Task Trigger(string jobName)
    {
        IScheduler scheduler = await Fixture.Factory.Services.GetRequiredService<ISchedulerFactory>()
            .GetScheduler(TestContext.Current.CancellationToken);
        await scheduler.TriggerJob(new JobKey(jobName), TestContext.Current.CancellationToken);
    }

    private async Task WaitForFile(Guid itemId, Func<EvidenceFile, bool> predicate)
    {
        for (int i = 0; i < 100; i++)
        {
            using IServiceScope scope = Fixture.Factory.Services.CreateScope();
            EvidenceDbContext db = scope.ServiceProvider.GetRequiredService<EvidenceDbContext>();
            EvidenceFile? file = await db.EvidenceFiles.AsNoTracking().SingleOrDefaultAsync(x => x.EvidenceItemId == EvidenceItemId.From(itemId), TestContext.Current.CancellationToken);
            if (file is not null && predicate(file)) return;
            await Task.Delay(50, TestContext.Current.CancellationToken);
        }
        throw new TimeoutException("Evidence background job did not reach the expected state.");
    }

    private sealed class FileUploadResponse
    {
        public Guid EvidenceItemId { get; init; }
        public Guid UploadAttemptId { get; init; }
        public string UploadMode { get; init; } = "";
        public string UploadUrl { get; init; } = "";
        public string Status { get; init; } = "";
    }
}
