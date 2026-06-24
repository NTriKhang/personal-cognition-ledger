using Common.Application.Messaging;

namespace PCL.Modules.Evidence.Application.Storage.CreateLocalStorageProfile;

public sealed record CreateLocalStorageProfileCommand(
    string Name,
    string RootDirectory,
    DateTimeOffset CreatedAt) : ICommand<Guid>;
