using PCL.Modules.Evidence.Domain.EvidenceItems;

namespace PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

public sealed record FileUploadReadModel(
    Guid EvidenceItemId,
    Guid UploadAttemptId,
    string UploadMode,
    string? UploadUrl,
    string UploadMethod,
    IReadOnlyDictionary<string, string> RequiredHeaders,
    DateTimeOffset ExpiresAt,
    string Status,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string ChecksumAlgorithm,
    string ChecksumValue
);

public sealed record FileDownloadReadModel(
    string? Url,
    Stream? Content,
    string FileName,
    string ContentType
);

internal static class FileUploadGuard
{
    public static Common.Domain.Result Validate(EvidenceItem? item, Guid sessionId, Guid ownerId)
    {
        if (item?.File is null)
            return Common.Domain.Result.Failure(EvidenceFileErrors.NotFound);
        if (item.SessionId != sessionId)
            return Common.Domain.Result.Failure(EvidenceFileErrors.SessionMismatch);
        if (item.OwnerId != ownerId)
            return Common.Domain.Result.Failure(EvidenceFileErrors.OwnerMismatch);
        return Common.Domain.Result.Success();
    }
}
