using Common.Application.Messaging;

namespace PCL.Modules.Evidence.Application.Storage.TestStorageProfile;

public sealed record TestStorageProfileCommand(Guid StorageProfileId) : ICommand;
