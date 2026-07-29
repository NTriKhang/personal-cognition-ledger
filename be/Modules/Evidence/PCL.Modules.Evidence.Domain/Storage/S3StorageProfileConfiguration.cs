namespace PCL.Modules.Evidence.Domain.Storage;

public sealed class S3StorageProfileConfiguration
{
    public const int MaximumBucketNameLength = 63;
    public const int MaximumRegionLength = 64;
    public const int MaximumKeyPrefixLength = 512;

    public StorageProfileId StorageProfileId { get; private set; }
    public string BucketName { get; private set; } = string.Empty;
    public string Region { get; private set; } = string.Empty;
    public string? KeyPrefix { get; private set; }

    private S3StorageProfileConfiguration()
    {
    }

    internal S3StorageProfileConfiguration(
        StorageProfileId storageProfileId,
        string bucketName,
        string region,
        string? keyPrefix)
    {
        StorageProfileId = storageProfileId;
        BucketName = bucketName;
        Region = region;
        KeyPrefix = keyPrefix;
    }
}
