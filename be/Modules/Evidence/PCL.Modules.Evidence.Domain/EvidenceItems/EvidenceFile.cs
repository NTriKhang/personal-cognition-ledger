using Common.Domain;

namespace PCL.Modules.Evidence.Domain.EvidenceItems;

public sealed class EvidenceFile
{
    public const int MaximumObjectKeyLength = 1024;
    public const int MaximumOriginalFileNameLength = 255;
    public const int MaximumContentTypeLength = 255;
    public const int MaximumChecksumAlgorithmLength = 32;
    public const int MaximumChecksumValueLength = 256;
    public const int MaximumVersionIdLength = 1024;
    public const int MaximumFailureReasonLength = 1000;

    public EvidenceItemId EvidenceItemId { get; private set; }
    public EvidenceFileUploadAttemptId UploadAttemptId { get; private set; }
    public string ObjectKey { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public EvidenceFileUploadStatus UploadStatus { get; private set; }
    public string? ChecksumAlgorithm { get; private set; }
    public string? ChecksumValue { get; private set; }
    public string? VersionId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UploadExpiresAt { get; private set; }
    public DateTimeOffset StatusChangedAt { get; private set; }
    public DateTimeOffset? UploadedAt { get; private set; }
    public string? FailureReason { get; private set; }

    private EvidenceFile()
    {
    }

    internal static Result<EvidenceFile> CreatePending(
        EvidenceItemId evidenceItemId,
        EvidenceFileUploadAttemptId uploadAttemptId,
        string objectKey,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        string? checksumAlgorithm,
        string? checksumValue,
        DateTimeOffset createdAt,
        DateTimeOffset uploadExpiresAt)
    {
        if (evidenceItemId.Value == Guid.Empty)
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.InvalidEvidenceItemId);

        if (uploadAttemptId.Value == Guid.Empty)
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.InvalidUploadAttemptId);

        if (string.IsNullOrWhiteSpace(objectKey))
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.InvalidObjectKey);

        if (string.IsNullOrWhiteSpace(originalFileName))
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.InvalidOriginalFileName);

        if (string.IsNullOrWhiteSpace(contentType))
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.InvalidContentType);

        string normalizedObjectKey = objectKey.Trim();
        string normalizedOriginalFileName = originalFileName.Trim();
        string normalizedContentType = contentType.Trim().ToLowerInvariant();

        if (normalizedObjectKey.Length > MaximumObjectKeyLength)
        {
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.InvalidObjectKey);
        }

        if (normalizedOriginalFileName.Length > MaximumOriginalFileNameLength)
        {
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.InvalidOriginalFileName);
        }

        if (normalizedContentType.Length > MaximumContentTypeLength)
        {
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.InvalidContentType);
        }

        if (!EvidenceFilePolicy.IsValidFileSize(fileSizeBytes))
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.InvalidFileSize);

        if (!EvidenceFilePolicy.IsAllowedContentType(normalizedContentType))
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.ContentTypeNotAllowed);

        if (uploadExpiresAt <= createdAt)
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.InvalidUploadExpiration);

        string? normalizedChecksumAlgorithm = NormalizeOptional(checksumAlgorithm);
        string? normalizedChecksumValue = NormalizeOptional(checksumValue);

        if (normalizedChecksumAlgorithm is null ||
            normalizedChecksumValue is null ||
            normalizedChecksumAlgorithm.Length > MaximumChecksumAlgorithmLength ||
            normalizedChecksumValue.Length > MaximumChecksumValueLength ||
            !EvidenceFilePolicy.IsValidChecksum(normalizedChecksumAlgorithm, normalizedChecksumValue))
        {
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.InvalidChecksum);
        }

        return Result.Success(new EvidenceFile
        {
            EvidenceItemId = evidenceItemId,
            UploadAttemptId = uploadAttemptId,
            ObjectKey = normalizedObjectKey,
            OriginalFileName = normalizedOriginalFileName,
            ContentType = normalizedContentType,
            FileSizeBytes = fileSizeBytes,
            UploadStatus = EvidenceFileUploadStatus.Pending,
            ChecksumAlgorithm = EvidenceFilePolicy.ChecksumAlgorithm,
            ChecksumValue = normalizedChecksumValue,
            CreatedAt = createdAt,
            UploadExpiresAt = uploadExpiresAt,
            StatusChangedAt = createdAt
        });
    }

    internal Result StartVerification(
        EvidenceFileUploadAttemptId uploadAttemptId,
        DateTimeOffset verifyingAt)
    {
        Result attemptResult = ValidateAttempt(uploadAttemptId);

        if (attemptResult.IsFailure)
            return attemptResult;

        if (UploadStatus is EvidenceFileUploadStatus.Verifying or EvidenceFileUploadStatus.Ready)
            return Result.Success();

        if (UploadStatus != EvidenceFileUploadStatus.Pending)
            return Result.Failure(EvidenceFileErrors.CannotStartVerification);

        if (verifyingAt < StatusChangedAt)
            return Result.Failure(EvidenceFileErrors.InvalidStatusChangeTime);

        UploadStatus = EvidenceFileUploadStatus.Verifying;
        StatusChangedAt = verifyingAt;
        FailureReason = null;

        return Result.Success();
    }

    internal Result MarkReady(
        EvidenceFileUploadAttemptId uploadAttemptId,
        DateTimeOffset uploadedAt,
        DateTimeOffset readyAt,
        string? versionId)
    {
        Result attemptResult = ValidateAttempt(uploadAttemptId);

        if (attemptResult.IsFailure)
            return attemptResult;

        if (UploadStatus == EvidenceFileUploadStatus.Ready)
            return Result.Success();

        if (UploadStatus != EvidenceFileUploadStatus.Verifying)
            return Result.Failure(EvidenceFileErrors.CannotMarkReady);

        if (uploadedAt < CreatedAt || uploadedAt > readyAt)
            return Result.Failure(EvidenceFileErrors.InvalidUploadedAt);

        if (readyAt < StatusChangedAt)
            return Result.Failure(EvidenceFileErrors.InvalidStatusChangeTime);

        string? normalizedVersionId = NormalizeOptional(versionId);

        if (normalizedVersionId?.Length > MaximumVersionIdLength)
            return Result.Failure(EvidenceFileErrors.InvalidVersionId);

        UploadStatus = EvidenceFileUploadStatus.Ready;
        UploadedAt = uploadedAt;
        VersionId = normalizedVersionId;
        StatusChangedAt = readyAt;
        FailureReason = null;

        return Result.Success();
    }

    internal Result MarkFailed(
        EvidenceFileUploadAttemptId uploadAttemptId,
        string failureReason,
        DateTimeOffset failedAt)
    {
        Result attemptResult = ValidateAttempt(uploadAttemptId);

        if (attemptResult.IsFailure)
            return attemptResult;

        if (UploadStatus == EvidenceFileUploadStatus.Failed)
            return Result.Success();

        if (UploadStatus is not EvidenceFileUploadStatus.Pending and
            not EvidenceFileUploadStatus.Verifying)
        {
            return Result.Failure(EvidenceFileErrors.CannotMarkFailed);
        }

        string? normalizedFailureReason = NormalizeOptional(failureReason);

        if (normalizedFailureReason is null ||
            normalizedFailureReason.Length > MaximumFailureReasonLength)
        {
            return Result.Failure(EvidenceFileErrors.InvalidFailureReason);
        }

        if (failedAt < StatusChangedAt)
            return Result.Failure(EvidenceFileErrors.InvalidStatusChangeTime);

        UploadStatus = EvidenceFileUploadStatus.Failed;
        FailureReason = normalizedFailureReason;
        StatusChangedAt = failedAt;

        return Result.Success();
    }

    internal Result Expire(
        EvidenceFileUploadAttemptId uploadAttemptId,
        DateTimeOffset expiredAt)
    {
        Result attemptResult = ValidateAttempt(uploadAttemptId);

        if (attemptResult.IsFailure)
            return attemptResult;

        if (UploadStatus == EvidenceFileUploadStatus.Expired)
            return Result.Success();

        if (UploadStatus != EvidenceFileUploadStatus.Pending)
            return Result.Failure(EvidenceFileErrors.CannotExpire);

        if (expiredAt < UploadExpiresAt)
            return Result.Failure(EvidenceFileErrors.UploadNotExpired);

        UploadStatus = EvidenceFileUploadStatus.Expired;
        StatusChangedAt = expiredAt;

        return Result.Success();
    }

    internal Result Cancel(
        EvidenceFileUploadAttemptId uploadAttemptId,
        DateTimeOffset cancelledAt)
    {
        Result attemptResult = ValidateAttempt(uploadAttemptId);

        if (attemptResult.IsFailure)
            return attemptResult;

        if (UploadStatus == EvidenceFileUploadStatus.Cancelled)
            return Result.Success();

        if (UploadStatus != EvidenceFileUploadStatus.Pending)
            return Result.Failure(EvidenceFileErrors.CannotCancel);

        if (cancelledAt < StatusChangedAt)
            return Result.Failure(EvidenceFileErrors.InvalidStatusChangeTime);

        UploadStatus = EvidenceFileUploadStatus.Cancelled;
        StatusChangedAt = cancelledAt;

        return Result.Success();
    }

    private Result ValidateAttempt(EvidenceFileUploadAttemptId uploadAttemptId)
    {
        if (uploadAttemptId.Value == Guid.Empty)
            return Result.Failure(EvidenceFileErrors.InvalidUploadAttemptId);

        return uploadAttemptId == UploadAttemptId
            ? Result.Success()
            : Result.Failure(EvidenceFileErrors.UploadAttemptMismatch);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
