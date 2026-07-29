using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Application.Storage;

internal static class StorageProfileReadModelMapper
{
    public static StorageProfileReadModel Map(StorageProfile profile) =>
        new(
            profile.Id.Value,
            profile.Name,
            profile.ProviderType.ToString(),
            profile.IsEnabled,
            profile.CreatedAt,
            profile.UpdatedAt,
            profile.LocalConfiguration is not null,
            profile.S3Configuration?.BucketName,
            profile.S3Configuration?.Region,
            profile.S3Configuration?.KeyPrefix);
}
