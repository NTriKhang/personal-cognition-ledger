using Common.Domain;

namespace PCL.Modules.Evidence.Domain.Storage;

public sealed class EvidenceStorageSettings
{
    public const int SingletonId = 1;

    public int Id { get; private set; }
    public StorageProfileId? ActiveStorageProfileId { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private EvidenceStorageSettings()
    {
    }

    public static EvidenceStorageSettings Create(DateTimeOffset createdAt) =>
        new()
        {
            Id = SingletonId,
            UpdatedAt = createdAt
        };

    public Result SelectProfile(StorageProfile profile, DateTimeOffset selectedAt)
    {
        if (!profile.IsEnabled)
            return Result.Failure(StorageProfileErrors.ProfileDisabled);

        if (!Enum.IsDefined(profile.ProviderType))
            return Result.Failure(StorageProfileErrors.InvalidProvider);

        ActiveStorageProfileId = profile.Id;
        UpdatedAt = selectedAt;

        return Result.Success();
    }
}
