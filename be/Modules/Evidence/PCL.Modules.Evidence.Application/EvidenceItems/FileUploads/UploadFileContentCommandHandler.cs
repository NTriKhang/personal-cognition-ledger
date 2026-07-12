using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Domain.EvidenceItems;

namespace PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

internal sealed class UploadFileContentCommandHandler(
    IEvidenceItemRepository items,
    IStorageProfileRepository profiles,
    IEvidenceFileStorageResolver resolver
) : ICommandHandler<UploadFileContentCommand>
{
    public async Task<Result> Handle(
        UploadFileContentCommand request,
        CancellationToken cancellationToken
    )
    {
        EvidenceItem? item = await items.GetAsync(
            EvidenceItemId.From(request.EvidenceItemId),
            cancellationToken
        );
        Result valid = FileUploadGuard.Validate(item, request.SessionId, request.OwnerId);
        if (valid.IsFailure)
            return valid;

        EvidenceFile file = item!.File!;
        if (file.UploadAttemptId.Value != request.UploadAttemptId)
            return Result.Failure(EvidenceFileErrors.UploadAttemptMismatch);

        if (DateTimeOffset.UtcNow >= file.UploadExpiresAt)
            return Result.Failure(EvidenceFileErrors.UploadExpired);

        var profile = await profiles.GetAsync(file.StorageProfileId, cancellationToken);
        if (profile is null)
            return Result.Failure(EvidenceFileErrors.StorageUnavailable);

        IEvidenceFileStorage storage = resolver.Resolve(profile.ProviderType);
        if (storage.ProviderType != Domain.Storage.EvidenceStorageProviderType.Local)
            return Result.Failure(EvidenceFileErrors.DirectUploadRequired);

        await storage.WriteAsync(
            profile,
            file.ObjectKey,
            request.Content,
            file.FileSizeBytes,
            file.ChecksumValue!,
            cancellationToken
        );
        return Result.Success();
    }
}
