namespace PCL.Modules.Evidence.Application.Storage;

public sealed record EvidenceStorageSettingsReadModel(
    StorageProfileReadModel? ActiveProfile,
    DateTimeOffset? UpdatedAt);
