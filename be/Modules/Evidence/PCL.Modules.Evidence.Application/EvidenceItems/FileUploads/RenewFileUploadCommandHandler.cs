using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Evidence.Application.Abstractions.Data;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Domain.EvidenceItems;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

internal sealed class RenewFileUploadCommandHandler(IEvidenceItemRepository items, IStorageProfileRepository profiles,
    IEvidenceFileStorageResolver resolver, IUnitOfWork unitOfWork) : ICommandHandler<RenewFileUploadCommand, FileUploadReadModel>
{
    public async Task<Result<FileUploadReadModel>> Handle(RenewFileUploadCommand r, CancellationToken ct)
    {
        EvidenceItem? item = await items.GetAsync(EvidenceItemId.From(r.EvidenceItemId), ct);
        Result valid = FileUploadGuard.Validate(item, r.SessionId, r.OwnerId);
        if (valid.IsFailure) return Result.Failure<FileUploadReadModel>(valid.Error);
        if (item!.IsRemoved) return Result.Failure<FileUploadReadModel>(EvidenceFileErrors.EvidenceItemRemoved);

        var expected = EvidenceFileUploadAttemptId.From(r.UploadAttemptId);
        if (item.File!.UploadAttemptId != expected)
        {
            EvidenceFileUploadAttempt? old = await items.GetAttemptAsync(expected, ct);
            if (old?.EvidenceItemId != item.Id || item.File.UploadStatus != EvidenceFileUploadStatus.Pending)
                return Result.Failure<FileUploadReadModel>(EvidenceFileErrors.UploadAttemptMismatch);
            return await Target(item, ct);
        }

        StorageProfile? profile = await profiles.GetAsync(item.File.StorageProfileId, ct);
        if (profile is null || !profile.IsEnabled)
            return Result.Failure<FileUploadReadModel>(EvidenceFileErrors.StorageUnavailable);
        var next = EvidenceFileUploadAttemptId.New();
        DateTimeOffset expires = r.RenewedAt.AddMinutes(15);
        string key = $"{item.OwnerId:N}/{item.SessionId:N}/{item.Id.Value:N}/{next.Value:N}";
        Result<EvidenceFileUploadAttempt> renewed = item.RenewFileUpload(expected, next, profile.Id, key, r.RenewedAt, expires);
        if (renewed.IsFailure) return Result.Failure<FileUploadReadModel>(renewed.Error);
        items.Add(renewed.Value);
        await unitOfWork.SaveChangesAsync(ct);
        return await Target(item, ct);
    }

    private async Task<Result<FileUploadReadModel>> Target(EvidenceItem item, CancellationToken ct)
    {
        StorageProfile? profile = await profiles.GetAsync(item.File!.StorageProfileId, ct);
        if (profile is null) return Result.Failure<FileUploadReadModel>(EvidenceFileErrors.StorageUnavailable);
        FileUploadTarget target = await resolver.Resolve(profile.ProviderType).CreateUploadTargetAsync(profile, item.File.ObjectKey,
            item.File.ContentType, item.File.FileSizeBytes, item.File.ChecksumValue!, item.File.UploadExpiresAt, ct);
        return Result.Success(InitializeFileUploadCommandHandler.ToModel(item, target));
    }
}
