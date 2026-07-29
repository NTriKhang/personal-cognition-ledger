using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Evidence.Application.Abstractions.Data;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Domain.EvidenceItems;
using PCL.Modules.Evidence.Domain.Storage;
using PCL.Modules.Session.Contracts.LSessions;
using System.Security.Cryptography;
using System.Text;

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
        string requestHash = Hash(request);
        EvidenceFileInitialization? prior = await itemRepository.GetInitializationAsync(request.OwnerId, request.IdempotencyKey, ct);
        if (prior is not null)
        {
            if (prior.RequestHash != requestHash)
                return Result.Failure<FileUploadReadModel>(EvidenceFileErrors.IdempotencyKeyConflict);
            EvidenceItem? existing = await itemRepository.GetAsync(prior.EvidenceItemId, ct);
            if (existing?.File is null)
                return Result.Failure<FileUploadReadModel>(EvidenceFileErrors.NotFound);
            if (existing.IsRemoved)
                return Result.Failure<FileUploadReadModel>(EvidenceFileErrors.EvidenceItemRemoved);
            StorageProfile? existingProfile = await profileRepository.GetAsync(existing.File.StorageProfileId, ct);
            if (existingProfile is null)
                return Result.Failure<FileUploadReadModel>(EvidenceFileErrors.StorageUnavailable);
            FileUploadTarget existingTarget = existing.File.UploadStatus == EvidenceFileUploadStatus.Pending
                ? await storageResolver.Resolve(existingProfile.ProviderType).CreateUploadTargetAsync(
                    existingProfile, existing.File.ObjectKey, existing.File.ContentType, existing.File.FileSizeBytes,
                    existing.File.ChecksumValue!, existing.File.UploadExpiresAt, ct)
                : new("Complete", null, "", new Dictionary<string, string>());
            return Result.Success(ToModel(existing, existingTarget));
        }

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
        itemRepository.Add(EvidenceFileInitialization.Create(request.OwnerId, request.IdempotencyKey, requestHash, item.Id, request.AddedAt));
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

    private static string Hash(InitializeFileUploadCommand r)
    {
        string value = string.Join('\n', r.SessionId, r.Caption?.Trim() ?? "", r.OriginalFileName.Trim(),
            r.ContentType.Trim().ToLowerInvariant(), r.FileSizeBytes, r.ChecksumAlgorithm.Trim().ToUpperInvariant(), r.ChecksumValue.Trim());
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
}
