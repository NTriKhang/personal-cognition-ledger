using Common.Application.Messaging;

namespace PCL.Modules.Evidence.Application.Storage.CreateS3StorageProfile;

public sealed record CreateS3StorageProfileCommand(
    string Name,
    string BucketName,
    string Region,
    string? KeyPrefix,
    DateTimeOffset CreatedAt) : ICommand<Guid>;
