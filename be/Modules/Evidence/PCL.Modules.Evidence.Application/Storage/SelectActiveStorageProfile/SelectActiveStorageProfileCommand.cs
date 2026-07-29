using Common.Application.Messaging;

namespace PCL.Modules.Evidence.Application.Storage.SelectActiveStorageProfile;

public sealed record SelectActiveStorageProfileCommand(
    Guid StorageProfileId,
    DateTimeOffset SelectedAt) : ICommand<StorageProfileReadModel>;
