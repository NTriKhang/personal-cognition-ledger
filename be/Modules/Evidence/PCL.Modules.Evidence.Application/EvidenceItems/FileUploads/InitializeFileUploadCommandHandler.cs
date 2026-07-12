using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Evidence.Application.Abstractions.Data;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Domain.EvidenceItems;
using PCL.Modules.Evidence.Domain.Storage;
using PCL.Modules.Session.Contracts.LSessions;

namespace PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

internal sealed class InitializeFileUploadCommandHandler(
    ISessionEvidenceAttachmentEligibilityChecker eligibility,
    IEvidenceStorageSettingsRepository settingsRepository,
    IStorageProfileRepository profileRepository,
    IEvidenceItemRepository itemRepository,
    IEvidenceFileStorageResolver storageResolver,
    IUnitOfWork unitOfWork
) : ICommandHandler<InitializeFileUploadCommand, FileUploadReadModel>
{
    public async Task<Result<FileUploadReadModel>> Handle(
        InitializeFileUploadCommand request,
        CancellationToken ct
    )
    {
        Result eligible = await eligibility.CheckAsync(request.SessionId, request.OwnerId, ct);
        if (eligible.IsFailure)
            return Result.Failure<FileUploadReadModel>(eligible.Error);

        EvidenceStorageSettings? settings = await settingsRepository.GetAsync(ct);
        if (settings?.ActiveStorageProfileId is null)
            return Result.Failure<FileUploadReadModel>(EvidenceFileErrors.StorageNotConfigured);

        StorageProfile? profile = await profileRepository.GetAsync(
            settings.ActiveStorageProfileId.Value,
            ct
        );
        if (profile is null || !profile.IsEnabled)
            return Result.Failure<FileUploadReadModel>(EvidenceFileErrors.StorageNotConfigured);

        Result<EvidenceItem> registered = EvidenceItem.RegisterFileReference(
            request.SessionId,
            request.OwnerId,
            request.Caption,
            request.AddedAt
        );
        if (registered.IsFailure)
            return Result.Failure<FileUploadReadModel>(registered.Error);

        EvidenceItem item = registered.Value;
        var attempt = EvidenceFileUploadAttemptId.New();
        DateTimeOffset expires = request.AddedAt.AddMinutes(15);
        string key =
            $"{request.OwnerId:N}/{request.SessionId:N}/{item.Id.Value:N}/{attempt.Value:N}";
        Result initialized = item.InitializeFileUpload(
            attempt,
            profile.Id,
            key,
            request.OriginalFileName,
            request.ContentType,
            request.FileSizeBytes,
            request.ChecksumAlgorithm,
            request.ChecksumValue,
            request.AddedAt,
            expires
        );
        if (initialized.IsFailure)
            return Result.Failure<FileUploadReadModel>(initialized.Error);

        itemRepository.Add(item);
        await unitOfWork.SaveChangesAsync(ct);
        FileUploadTarget target = await storageResolver
            .Resolve(profile.ProviderType)
            .CreateUploadTargetAsync(
                profile,
                key,
                item.File!.ContentType,
                item.File.FileSizeBytes,
                item.File.ChecksumValue!,
                expires,
                ct
            );

        return Result.Success(ToModel(item, target));
    }

    internal static FileUploadReadModel ToModel(EvidenceItem item, FileUploadTarget target) =>
        new(
            item.Id.Value,
            item.File!.UploadAttemptId.Value,
            target.Mode,
            target.Url,
            target.Method,
            target.RequiredHeaders,
            item.File.UploadExpiresAt,
            item.File.UploadStatus.ToString(),
            item.File.OriginalFileName,
            item.File.ContentType,
            item.File.FileSizeBytes,
            item.File.ChecksumAlgorithm!,
            item.File.ChecksumValue!
        );
}
