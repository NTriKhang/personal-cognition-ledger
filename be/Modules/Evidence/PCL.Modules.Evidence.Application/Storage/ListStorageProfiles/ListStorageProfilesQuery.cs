using Common.Application.Messaging;

namespace PCL.Modules.Evidence.Application.Storage.ListStorageProfiles;

public sealed record ListStorageProfilesQuery
    : IQuery<IReadOnlyCollection<StorageProfileReadModel>>;
