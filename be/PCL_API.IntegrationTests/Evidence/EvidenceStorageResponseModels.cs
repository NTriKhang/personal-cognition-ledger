namespace PCL_API.IntegrationTests.Evidence;

internal sealed class StorageProfileResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ProviderType { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public bool HasLocalConfiguration { get; init; }
    public string? BucketName { get; init; }
    public string? Region { get; init; }
    public string? KeyPrefix { get; init; }
}

internal sealed class EvidenceStorageSettingsResponse
{
    public StorageProfileResponse? ActiveProfile { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
