namespace PCL.Modules.Evidence.Application.Storage;

public sealed record StorageProfileReadModel(
    Guid Id,
    string Name,
    string ProviderType,
    bool IsEnabled,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    bool HasLocalConfiguration,
    string? BucketName,
    string? Region,
    string? KeyPrefix);
