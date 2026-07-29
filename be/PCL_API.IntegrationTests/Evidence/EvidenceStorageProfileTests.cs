using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PCL_API.IntegrationTests.Infrastructure;
using PCL_API.IntegrationTests.TestData;
using Xunit;

namespace PCL_API.IntegrationTests.Evidence;

public sealed class EvidenceStorageProfileTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Local_profile_should_create_list_test_select_and_become_active()
    {
        string rootDirectory = Path.Combine(Fixture.LocalStorageRoot, Guid.NewGuid().ToString("N"));
        Guid profileId = await CreateLocalAsync(rootDirectory, "Local evidence");

        Assert.True(Directory.Exists(rootDirectory));
        StorageProfileResponse profile = Assert.Single(await ListProfilesAsync());
        Assert.Equal(profileId, profile.Id);
        Assert.Equal("Local evidence", profile.Name);
        Assert.Equal("Local", profile.ProviderType);
        Assert.True(profile.HasLocalConfiguration);

        HttpResponseMessage tested = await Client.PostAsync(
            $"/admin/evidence-storage/profiles/{profileId}/test",
            content: null,
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, tested.StatusCode);

        HttpResponseMessage selected = await Client.PutAsJsonAsync(
            "/admin/evidence-storage/settings/active-profile",
            TestDataBuilder.SelectActiveStorageProfileRequest(profileId),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, selected.StatusCode);

        EvidenceStorageSettingsResponse settings = await GetSettingsAsync();
        Assert.Equal(profileId, settings.ActiveProfile?.Id);
        Assert.NotNull(settings.UpdatedAt);
    }

    [Fact]
    public async Task Local_profile_should_reject_duplicate_outside_and_unavailable_directories()
    {
        string validRoot = Path.Combine(Fixture.LocalStorageRoot, "valid");
        await CreateLocalAsync(validRoot, "Repeated name");

        HttpResponseMessage duplicate = await CreateLocalResponseAsync(
            Path.Combine(Fixture.LocalStorageRoot, "other"), "Repeated name");
        await duplicate.ShouldBeProblemAsync(HttpStatusCode.Conflict, "StorageProfile.NameAlreadyExists");

        string outsideRoot = Path.Combine(Path.GetTempPath(), "pcl-outside", Guid.NewGuid().ToString("N"));
        HttpResponseMessage outside = await CreateLocalResponseAsync(outsideRoot, "Outside root");
        await outside.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "StorageProfile.LocalRootNotAllowed");

        string filePath = Path.Combine(Fixture.LocalStorageRoot, "not-a-directory");
        await File.WriteAllTextAsync(filePath, "occupied", TestContext.Current.CancellationToken);
        HttpResponseMessage unavailable = await CreateLocalResponseAsync(filePath, "Unavailable root");
        await unavailable.ShouldBeProblemAsync(
            HttpStatusCode.BadRequest, "StorageProfile.LocalDirectoryUnavailable");
    }

    [Theory]
    [InlineData("", "root")]
    [InlineData("name", "")]
    public async Task Local_profile_should_validate_required_fields(string name, string rootDirectory)
    {
        string resolvedRoot = rootDirectory.Length == 0
            ? rootDirectory
            : Path.Combine(Fixture.LocalStorageRoot, rootDirectory);

        HttpResponseMessage response = await CreateLocalResponseAsync(resolvedRoot, name);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "General.Validation");
        problem.ShouldContainValidationErrors();
    }

    [Fact]
    public async Task AmazonS3_profile_should_use_configurable_verification()
    {
        Guid profileId = await CreateS3Async();
        StorageProfileResponse profile = Assert.Single(await ListProfilesAsync());
        Assert.Equal(profileId, profile.Id);
        Assert.Equal("AmazonS3", profile.ProviderType);
        Assert.Equal("pcl-integration-tests", profile.BucketName);
        Assert.Equal("ap-southeast-1", profile.Region);
        Assert.Equal("evidence/tests", profile.KeyPrefix);

        HttpResponseMessage successfulTest = await Client.PostAsync(
            $"/admin/evidence-storage/profiles/{profileId}/test",
            content: null,
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, successfulTest.StatusCode);

        HttpResponseMessage successfulSelection = await Client.PutAsJsonAsync(
            "/admin/evidence-storage/settings/active-profile",
            TestDataBuilder.SelectActiveStorageProfileRequest(profileId),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, successfulSelection.StatusCode);

        HttpResponseMessage secondCreation = await Client.PostAsJsonAsync(
            "/admin/evidence-storage/profiles/amazon-s3",
            TestDataBuilder.CreateS3StorageProfileRequest(name: "Fallback S3 Storage"),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, secondCreation.StatusCode);
        Guid secondProfileId = await secondCreation.Content.ReadFromJsonAsync<Guid>(
            TestContext.Current.CancellationToken);

        FakeS3StorageProfileVerifier verifier = Fixture.Factory.Services
            .GetRequiredService<FakeS3StorageProfileVerifier>();
        verifier.ShouldSucceed = false;

        HttpResponseMessage tested = await Client.PostAsync(
            $"/admin/evidence-storage/profiles/{secondProfileId}/test",
            content: null,
            TestContext.Current.CancellationToken);
        await tested.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "StorageProfile.AmazonS3Unavailable");

        HttpResponseMessage selected = await Client.PutAsJsonAsync(
            "/admin/evidence-storage/settings/active-profile",
            TestDataBuilder.SelectActiveStorageProfileRequest(secondProfileId),
            TestContext.Current.CancellationToken);
        await selected.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "StorageProfile.AmazonS3Unavailable");
        Assert.Equal(profileId, (await GetSettingsAsync()).ActiveProfile?.Id);
    }

    [Fact]
    public async Task AmazonS3_creation_should_reject_unavailable_provider_and_invalid_fields()
    {
        FakeS3StorageProfileVerifier verifier = Fixture.Factory.Services
            .GetRequiredService<FakeS3StorageProfileVerifier>();
        verifier.ShouldSucceed = false;

        HttpResponseMessage unavailable = await Client.PostAsJsonAsync(
            "/admin/evidence-storage/profiles/amazon-s3",
            TestDataBuilder.CreateS3StorageProfileRequest(),
            TestContext.Current.CancellationToken);
        await unavailable.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "StorageProfile.AmazonS3Unavailable");
        Assert.Empty(await ListProfilesAsync());

        verifier.ShouldSucceed = true;
        var invalidRequest = new { Name = "", BucketName = "", Region = "", KeyPrefix = new string('x', 513) };
        HttpResponseMessage invalid = await Client.PostAsJsonAsync(
            "/admin/evidence-storage/profiles/amazon-s3",
            invalidRequest,
            TestContext.Current.CancellationToken);
        var problem = await invalid.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "General.Validation");
        problem.ShouldContainValidationErrors();
    }

    [Fact]
    public async Task Test_and_select_should_report_missing_profiles()
    {
        Guid missingId = Guid.NewGuid();
        HttpResponseMessage tested = await Client.PostAsync(
            $"/admin/evidence-storage/profiles/{missingId}/test",
            content: null,
            TestContext.Current.CancellationToken);
        await tested.ShouldBeProblemAsync(HttpStatusCode.NotFound, "StorageProfile.NotFound");

        HttpResponseMessage selected = await Client.PutAsJsonAsync(
            "/admin/evidence-storage/settings/active-profile",
            TestDataBuilder.SelectActiveStorageProfileRequest(missingId),
            TestContext.Current.CancellationToken);
        await selected.ShouldBeProblemAsync(HttpStatusCode.NotFound, "StorageProfile.NotFound");
    }

    private async Task<Guid> CreateLocalAsync(string rootDirectory, string name)
    {
        HttpResponseMessage response = await CreateLocalResponseAsync(rootDirectory, name);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private Task<HttpResponseMessage> CreateLocalResponseAsync(string rootDirectory, string name) =>
        Client.PostAsJsonAsync(
            "/admin/evidence-storage/profiles/local",
            TestDataBuilder.CreateLocalStorageProfileRequest(rootDirectory, name),
            TestContext.Current.CancellationToken);

    private async Task<Guid> CreateS3Async()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            "/admin/evidence-storage/profiles/amazon-s3",
            TestDataBuilder.CreateS3StorageProfileRequest(),
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<Guid>(TestContext.Current.CancellationToken);
    }

    private async Task<IReadOnlyCollection<StorageProfileResponse>> ListProfilesAsync() =>
        await Client.GetFromJsonAsync<IReadOnlyCollection<StorageProfileResponse>>(
            "/admin/evidence-storage/profiles",
            TestContext.Current.CancellationToken)
        ?? throw new Xunit.Sdk.XunitException("The storage profile list response was empty.");

    private async Task<EvidenceStorageSettingsResponse> GetSettingsAsync() =>
        await Client.GetFromJsonAsync<EvidenceStorageSettingsResponse>(
            "/admin/evidence-storage/settings",
            TestContext.Current.CancellationToken)
        ?? throw new Xunit.Sdk.XunitException("The storage settings response was empty.");
}
