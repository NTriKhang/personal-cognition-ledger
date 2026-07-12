using Common.Domain;
using PCL.Modules.Evidence.Domain.Storage;
using PCL.Modules.Evidence.Domain.EvidenceItems.Events;

namespace PCL.Modules.Evidence.Domain.EvidenceItems;

public sealed class EvidenceItem : Entity
{
    public const int MaximumContentLength = 10000;
    public const int MaximumReferenceLength = 2048;

    public EvidenceItemId Id { get; private set; }
    public Guid SessionId { get; private set; }
    public Guid OwnerId { get; private set; }
    public EvidenceItemType Type { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTimeOffset AddedAt { get; private set; }
    public DateTimeOffset? RemovedAt { get; private set; }
    public Guid? RemovedBy { get; private set; }
    public string? RemovalReason { get; private set; }
    public EvidenceFile? File { get; private set; }

    public bool IsRemoved => RemovedAt.HasValue;

    private EvidenceItem()
    {
    }

    public static Result<EvidenceItem> Register(
        Guid sessionId,
        Guid ownerId,
        EvidenceItemType type,
        string content,
        DateTimeOffset addedAt)
    {
        if (type == EvidenceItemType.FileReference)
            return Result.Failure<EvidenceItem>(EvidenceItemErrors.FileReferenceRequiresUploadInitialization);

        if (sessionId == Guid.Empty)
            return Result.Failure<EvidenceItem>(EvidenceItemErrors.InvalidSessionId);

        if (ownerId == Guid.Empty)
            return Result.Failure<EvidenceItem>(EvidenceItemErrors.InvalidOwnerId);

        if (!Enum.IsDefined(type))
            return Result.Failure<EvidenceItem>(EvidenceItemErrors.InvalidType);

        if (string.IsNullOrWhiteSpace(content))
            return Result.Failure<EvidenceItem>(EvidenceItemErrors.InvalidContent);

        string normalizedContent = content.Trim();
        int maximumLength = type == EvidenceItemType.Note
            ? MaximumContentLength
            : MaximumReferenceLength;

        if (normalizedContent.Length > maximumLength)
            return Result.Failure<EvidenceItem>(EvidenceItemErrors.InvalidContent);

        if (type == EvidenceItemType.Link && !IsValidLink(normalizedContent))
            return Result.Failure<EvidenceItem>(EvidenceItemErrors.InvalidLink);

        return Result.Success(Create(sessionId, ownerId, type, normalizedContent, addedAt));
    }

    public static Result<EvidenceItem> RegisterFileReference(
        Guid sessionId,
        Guid ownerId,
        string? caption,
        DateTimeOffset addedAt)
    {
        if (sessionId == Guid.Empty)
            return Result.Failure<EvidenceItem>(EvidenceItemErrors.InvalidSessionId);

        if (ownerId == Guid.Empty)
            return Result.Failure<EvidenceItem>(EvidenceItemErrors.InvalidOwnerId);

        string normalizedCaption = NormalizeOptional(caption) ?? string.Empty;

        if (normalizedCaption.Length > MaximumReferenceLength)
            return Result.Failure<EvidenceItem>(EvidenceItemErrors.InvalidFileReferenceCaption);

        return Result.Success(Create(
            sessionId,
            ownerId,
            EvidenceItemType.FileReference,
            normalizedCaption,
            addedAt));
    }

    public Result Remove(Guid removedBy, DateTimeOffset removedAt, string? removalReason)
    {
        if (removedBy == Guid.Empty)
            return Result.Failure(EvidenceItemErrors.InvalidRemovedBy);

        if (removedBy != OwnerId)
            return Result.Failure(EvidenceItemErrors.OwnerMismatch);

        if (IsRemoved)
            return Result.Success();

        if (removedAt < AddedAt)
            return Result.Failure(EvidenceItemErrors.InvalidRemovalTime);

        string? normalizedRemovalReason = NormalizeOptional(removalReason);

        if (normalizedRemovalReason?.Length > EvidenceItemErrors.MaximumRemovalReasonLength)
            return Result.Failure(EvidenceItemErrors.InvalidRemovalReason);

        RemovedAt = removedAt;
        RemovedBy = removedBy;
        RemovalReason = normalizedRemovalReason;

        Raise(new EvidenceItemRemovedDomainEvent(
            Id,
            SessionId,
            OwnerId,
            RemovedBy.Value,
            RemovedAt.Value,
            RemovalReason));

        return Result.Success();
    }

    public Result InitializeFileUpload(
        EvidenceFileUploadAttemptId uploadAttemptId,
        StorageProfileId storageProfileId,
        string objectKey,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        string? checksumAlgorithm,
        string? checksumValue,
        DateTimeOffset createdAt,
        DateTimeOffset uploadExpiresAt)
    {
        if (IsRemoved)
            return Result.Failure(EvidenceFileErrors.EvidenceItemRemoved);

        if (Type != EvidenceItemType.FileReference)
            return Result.Failure(EvidenceFileErrors.EvidenceItemTypeMismatch);

        if (File is not null)
            return Result.Failure(EvidenceFileErrors.AlreadyAttached);

        Result<EvidenceFile> fileResult = EvidenceFile.CreatePending(
            Id,
            uploadAttemptId,
            storageProfileId,
            objectKey,
            originalFileName,
            contentType,
            fileSizeBytes,
            checksumAlgorithm,
            checksumValue,
            createdAt,
            uploadExpiresAt);

        if (fileResult.IsFailure)
            return fileResult;

        File = fileResult.Value;

        return Result.Success();
    }

    public Result StartFileVerification(
        EvidenceFileUploadAttemptId uploadAttemptId,
        DateTimeOffset verifyingAt) =>
        File is null
            ? Result.Failure(EvidenceFileErrors.NotAttached)
            : File.StartVerification(uploadAttemptId, verifyingAt);

    public Result MarkFileReady(
        EvidenceFileUploadAttemptId uploadAttemptId,
        DateTimeOffset uploadedAt,
        DateTimeOffset readyAt,
        string? versionId) =>
        File is null
            ? Result.Failure(EvidenceFileErrors.NotAttached)
            : File.MarkReady(uploadAttemptId, uploadedAt, readyAt, versionId);

    public Result MarkFileFailed(
        EvidenceFileUploadAttemptId uploadAttemptId,
        string failureReason,
        DateTimeOffset failedAt) =>
        File is null
            ? Result.Failure(EvidenceFileErrors.NotAttached)
            : File.MarkFailed(uploadAttemptId, failureReason, failedAt);

    public Result ExpireFileUpload(
        EvidenceFileUploadAttemptId uploadAttemptId,
        DateTimeOffset expiredAt) =>
        File is null
            ? Result.Failure(EvidenceFileErrors.NotAttached)
            : File.Expire(uploadAttemptId, expiredAt);

    public Result CancelFileUpload(
        EvidenceFileUploadAttemptId uploadAttemptId,
        DateTimeOffset cancelledAt) =>
        File is null
            ? Result.Failure(EvidenceFileErrors.NotAttached)
            : File.Cancel(uploadAttemptId, cancelledAt);

    private static EvidenceItem Create(
        Guid sessionId,
        Guid ownerId,
        EvidenceItemType type,
        string content,
        DateTimeOffset addedAt)
    {
        var evidenceItem = new EvidenceItem
        {
            Id = EvidenceItemId.New(),
            SessionId = sessionId,
            OwnerId = ownerId,
            Type = type,
            Content = content,
            AddedAt = addedAt
        };

        evidenceItem.Raise(new EvidenceItemAddedDomainEvent(
            evidenceItem.Id,
            evidenceItem.SessionId,
            evidenceItem.OwnerId,
            evidenceItem.Type,
            evidenceItem.Content,
            evidenceItem.AddedAt));

        return evidenceItem;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool IsValidLink(string content) =>
        Uri.TryCreate(content, UriKind.Absolute, out Uri? uri) &&
        (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
         string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));
}
