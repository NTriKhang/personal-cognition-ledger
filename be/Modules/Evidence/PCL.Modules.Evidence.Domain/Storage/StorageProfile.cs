using Common.Domain;

namespace PCL.Modules.Evidence.Domain.Storage;

public sealed class StorageProfile : Entity
{
    public const int MaximumNameLength = 100;

    public StorageProfileId Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public EvidenceStorageProviderType ProviderType { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public LocalStorageProfileConfiguration? LocalConfiguration { get; private set; }
    public S3StorageProfileConfiguration? S3Configuration { get; private set; }

    private StorageProfile()
    {
    }

    public static Result<StorageProfile> CreateLocal(
        string name,
        string rootDirectory,
        DateTimeOffset createdAt)
    {
        string? normalizedName = NormalizeRequired(name);

        if (normalizedName is null || normalizedName.Length > MaximumNameLength)
            return Result.Failure<StorageProfile>(StorageProfileErrors.InvalidName);

        string? normalizedRootDirectory = NormalizeRequired(rootDirectory);

        if (normalizedRootDirectory is null ||
            normalizedRootDirectory.Length > LocalStorageProfileConfiguration.MaximumRootDirectoryLength)
        {
            return Result.Failure<StorageProfile>(StorageProfileErrors.InvalidRootDirectory);
        }

        var profile = new StorageProfile
        {
            Id = StorageProfileId.New(),
            Name = normalizedName,
            ProviderType = EvidenceStorageProviderType.Local,
            IsEnabled = true,
            CreatedAt = createdAt
        };

        profile.LocalConfiguration = new LocalStorageProfileConfiguration(
            profile.Id,
            normalizedRootDirectory);

        return Result.Success(profile);
    }

    public static Result<StorageProfile> CreateAmazonS3(
        string name,
        string bucketName,
        string region,
        string? keyPrefix,
        DateTimeOffset createdAt)
    {
        string? normalizedName = NormalizeRequired(name);

        if (normalizedName is null || normalizedName.Length > MaximumNameLength)
            return Result.Failure<StorageProfile>(StorageProfileErrors.InvalidName);

        string? normalizedBucketName = NormalizeRequired(bucketName);

        if (normalizedBucketName is null ||
            normalizedBucketName.Length > S3StorageProfileConfiguration.MaximumBucketNameLength)
        {
            return Result.Failure<StorageProfile>(StorageProfileErrors.InvalidBucketName);
        }

        string? normalizedRegion = NormalizeRequired(region);

        if (normalizedRegion is null ||
            normalizedRegion.Length > S3StorageProfileConfiguration.MaximumRegionLength)
        {
            return Result.Failure<StorageProfile>(StorageProfileErrors.InvalidRegion);
        }

        string? normalizedKeyPrefix = NormalizeKeyPrefix(keyPrefix);

        if (normalizedKeyPrefix?.Length > S3StorageProfileConfiguration.MaximumKeyPrefixLength ||
            normalizedKeyPrefix?.Split('/').Any(segment => segment == "..") == true)
        {
            return Result.Failure<StorageProfile>(StorageProfileErrors.InvalidKeyPrefix);
        }

        var profile = new StorageProfile
        {
            Id = StorageProfileId.New(),
            Name = normalizedName,
            ProviderType = EvidenceStorageProviderType.AmazonS3,
            IsEnabled = true,
            CreatedAt = createdAt
        };

        profile.S3Configuration = new S3StorageProfileConfiguration(
            profile.Id,
            normalizedBucketName.ToLowerInvariant(),
            normalizedRegion.ToLowerInvariant(),
            normalizedKeyPrefix);

        return Result.Success(profile);
    }

    public Result Disable(DateTimeOffset disabledAt)
    {
        if (!IsEnabled)
            return Result.Success();

        if (disabledAt < CreatedAt)
            return Result.Failure(StorageProfileErrors.InvalidProvider);

        IsEnabled = false;
        UpdatedAt = disabledAt;

        return Result.Success();
    }

    private static string? NormalizeRequired(string value)
    {
        string normalized = value.Trim();

        return string.IsNullOrWhiteSpace(normalized)
            ? null
            : normalized;
    }

    private static string? NormalizeKeyPrefix(string? keyPrefix)
    {
        if (string.IsNullOrWhiteSpace(keyPrefix))
            return null;

        string normalized = keyPrefix
            .Trim()
            .Replace('\\', '/')
            .Trim('/');

        return string.IsNullOrWhiteSpace(normalized)
            ? null
            : normalized;
    }
}
