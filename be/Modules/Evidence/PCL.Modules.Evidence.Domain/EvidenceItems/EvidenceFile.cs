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
    public DateTimeOffset? UploadedAt { get; private set; }
    public string? FailureReason { get; private set; }

    private EvidenceFile()
    {
    }

    internal static Result<EvidenceFile> CreatePending(
        EvidenceItemId evidenceItemId,
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

        if (string.IsNullOrWhiteSpace(objectKey))
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.InvalidObjectKey);

        if (string.IsNullOrWhiteSpace(originalFileName))
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.InvalidOriginalFileName);

        if (string.IsNullOrWhiteSpace(contentType))
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.InvalidContentType);

        string normalizedObjectKey = objectKey.Trim();
        string normalizedOriginalFileName = originalFileName.Trim();
        string normalizedContentType = contentType.Trim();

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

        if (fileSizeBytes < 0)
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.InvalidFileSize);

        if (uploadExpiresAt <= createdAt)
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.InvalidUploadExpiration);

        string? normalizedChecksumAlgorithm = NormalizeOptional(checksumAlgorithm);
        string? normalizedChecksumValue = NormalizeOptional(checksumValue);

        if ((normalizedChecksumAlgorithm is null) != (normalizedChecksumValue is null) ||
            normalizedChecksumAlgorithm?.Length > MaximumChecksumAlgorithmLength ||
            normalizedChecksumValue?.Length > MaximumChecksumValueLength)
        {
            return Result.Failure<EvidenceFile>(EvidenceFileErrors.InvalidChecksum);
        }

        return Result.Success(new EvidenceFile
        {
            EvidenceItemId = evidenceItemId,
            ObjectKey = normalizedObjectKey,
            OriginalFileName = normalizedOriginalFileName,
            ContentType = normalizedContentType,
            FileSizeBytes = fileSizeBytes,
            UploadStatus = EvidenceFileUploadStatus.Pending,
            ChecksumAlgorithm = normalizedChecksumAlgorithm,
            ChecksumValue = normalizedChecksumValue,
            CreatedAt = createdAt,
            UploadExpiresAt = uploadExpiresAt
        });
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
