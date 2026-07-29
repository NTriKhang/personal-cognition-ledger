using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Domain.EvidenceItems;

namespace PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

internal sealed class DownloadEvidenceFileQueryHandler(
    IEvidenceItemRepository items,
    IStorageProfileRepository profiles,
    IEvidenceFileStorageResolver resolver
) : IQueryHandler<DownloadEvidenceFileQuery, FileDownloadReadModel>
{
    public async Task<Result<FileDownloadReadModel>> Handle(
        DownloadEvidenceFileQuery r,
        CancellationToken ct
    )
    {
        EvidenceItem? item = await items.GetAsync(EvidenceItemId.From(r.EvidenceItemId), ct);
        Result valid = FileUploadGuard.Validate(item, r.SessionId, r.OwnerId);
        if (valid.IsFailure)
            return Result.Failure<FileDownloadReadModel>(valid.Error);
        EvidenceFile file = item!.File!;
        if (file.UploadStatus != EvidenceFileUploadStatus.Ready)
            return Result.Failure<FileDownloadReadModel>(EvidenceFileErrors.NotReady);
        var profile = await profiles.GetAsync(file.StorageProfileId, ct);
        if (profile is null)
            return Result.Failure<FileDownloadReadModel>(EvidenceFileErrors.StorageUnavailable);
        FileDownloadTarget target = await resolver
            .Resolve(profile.ProviderType)
            .CreateDownloadTargetAsync(
                profile,
                file.ObjectKey,
                file.OriginalFileName,
                file.ContentType,
                ct
            );
        return Result.Success(
            new FileDownloadReadModel(
                target.Url,
                target.Content,
                file.OriginalFileName,
                file.ContentType
            )
        );
    }
}
