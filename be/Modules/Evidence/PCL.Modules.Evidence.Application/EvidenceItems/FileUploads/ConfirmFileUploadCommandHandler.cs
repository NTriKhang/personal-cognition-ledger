using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Evidence.Application.Abstractions.Data;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Domain.EvidenceItems;

namespace PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

internal sealed class ConfirmFileUploadCommandHandler(
    IEvidenceItemRepository items,
    IStorageProfileRepository profiles,
    IEvidenceFileStorageResolver resolver,
    IUnitOfWork unitOfWork
) : ICommandHandler<ConfirmFileUploadCommand, FileUploadReadModel>
{
    public async Task<Result<FileUploadReadModel>> Handle(
        ConfirmFileUploadCommand r,
        CancellationToken ct
    )
    {
        EvidenceItem? item = await items.GetAsync(EvidenceItemId.From(r.EvidenceItemId), ct);
        Result valid = FileUploadGuard.Validate(item, r.SessionId, r.OwnerId);
        if (valid.IsFailure)
            return Result.Failure<FileUploadReadModel>(valid.Error);
        EvidenceFile file = item!.File!;
        var attempt = EvidenceFileUploadAttemptId.From(r.UploadAttemptId);
        if (r.ConfirmedAt >= file.UploadExpiresAt)
        {
            Result expired = item.ExpireFileUpload(attempt, r.ConfirmedAt);
            if (expired.IsSuccess)
                await unitOfWork.SaveChangesAsync(ct);
            return Result.Failure<FileUploadReadModel>(
                expired.IsFailure ? expired.Error : EvidenceFileErrors.UploadExpired
            );
        }
        if (file.UploadStatus == EvidenceFileUploadStatus.Ready)
            return Result.Success(Model(item));
        var profile = await profiles.GetAsync(file.StorageProfileId, ct);
        if (profile is null)
            return Result.Failure<FileUploadReadModel>(EvidenceFileErrors.StorageUnavailable);
        StoredFileMetadata? metadata = await resolver
            .Resolve(profile.ProviderType)
            .GetMetadataAsync(profile, file.ObjectKey, ct);
        if (metadata is null)
            return Result.Failure<FileUploadReadModel>(EvidenceFileErrors.ContentUploadRequired);
        Result started = item.StartFileVerification(attempt, r.ConfirmedAt);
        if (started.IsFailure)
            return Result.Failure<FileUploadReadModel>(started.Error);
        bool contentTypeMatches =
            metadata.ContentType == "application/octet-stream"
            || string.Equals(
                metadata.ContentType,
                file.ContentType,
                StringComparison.OrdinalIgnoreCase
            );
        if (
            metadata.Size != file.FileSizeBytes
            || !contentTypeMatches
            || !string.Equals(metadata.Checksum, file.ChecksumValue, StringComparison.Ordinal)
        )
        {
            item.MarkFileFailed(
                attempt,
                EvidenceFileErrors.MetadataMismatch.Description,
                r.ConfirmedAt
            );
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Failure<FileUploadReadModel>(EvidenceFileErrors.MetadataMismatch);
        }
        Result ready = item.MarkFileReady(
            attempt,
            metadata.UploadedAt,
            r.ConfirmedAt,
            metadata.VersionId
        );
        if (ready.IsFailure)
            return Result.Failure<FileUploadReadModel>(ready.Error);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(Model(item));
    }

    private static FileUploadReadModel Model(EvidenceItem item) =>
        InitializeFileUploadCommandHandler.ToModel(
            item,
            new("Complete", null, "", new Dictionary<string, string>())
        );
}
