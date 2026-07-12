using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
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
        HttpResponseMessage initialized = await Client.PostAsJsonAsync(
            $"/lsessions/{session}/evidence-items/file-uploads",
            new
            {
                OwnerId = owner,
                Caption = "receipt",
                OriginalFileName = "receipt.txt",
                ContentType = "text/plain",
                FileSizeBytes = content.LongLength,
                ChecksumAlgorithm = "SHA256",
                ChecksumValue = checksum,
            },
            TestContext.Current.CancellationToken
        );
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
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"/lsessions/{session}/evidence-items/file-uploads",
            new
            {
                OwnerId = owner,
                Caption = (string?)null,
                OriginalFileName = "x.txt",
                ContentType = "text/plain",
                FileSizeBytes = 1,
                ChecksumAlgorithm = "SHA256",
                ChecksumValue = Convert.ToBase64String(SHA256.HashData(data)),
            },
            TestContext.Current.CancellationToken
        );
        await response.ShouldBeProblemAsync(
            HttpStatusCode.Conflict,
            "EvidenceFile.StorageNotConfigured"
        );
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

    private sealed class FileUploadResponse
    {
        public Guid EvidenceItemId { get; init; }
        public Guid UploadAttemptId { get; init; }
        public string UploadMode { get; init; } = "";
        public string UploadUrl { get; init; } = "";
        public string Status { get; init; } = "";
    }
}
