namespace PCL.Modules.Evidence.Domain.Storage;

public sealed class LocalStorageProfileConfiguration
{
    public const int MaximumRootDirectoryLength = 2048;

    public StorageProfileId StorageProfileId { get; private set; }
    public string RootDirectory { get; private set; } = string.Empty;

    private LocalStorageProfileConfiguration()
    {
    }

    internal LocalStorageProfileConfiguration(
        StorageProfileId storageProfileId,
        string rootDirectory)
    {
        StorageProfileId = storageProfileId;
        RootDirectory = rootDirectory;
    }
}
