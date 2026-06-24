namespace PCL.Modules.Evidence.Domain.Storage;

public readonly record struct StorageProfileId(Guid Value)
{
    public static StorageProfileId New() => new(Guid.NewGuid());

    public static StorageProfileId From(Guid value) => new(value);
}
