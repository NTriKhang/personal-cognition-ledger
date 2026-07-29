using Common.Domain;

namespace PCL.Modules.Evidence.Domain.EvidenceItems;

public static class EvidenceFileErrors
{
    public static readonly Error InvalidEvidenceItemId =
        Error.Problem(
            "EvidenceFile.InvalidEvidenceItemId",
            "EvidenceItemId cannot be empty.");

    public static readonly Error InvalidUploadAttemptId =
        Error.Problem(
            "EvidenceFile.InvalidUploadAttemptId",
            "UploadAttemptId cannot be empty.");

    public static readonly Error InvalidStorageProfileId =
        Error.Problem("EvidenceFile.InvalidStorageProfileId", "StorageProfileId cannot be empty.");

    public static readonly Error StorageNotConfigured =
        Error.Conflict("EvidenceFile.StorageNotConfigured", "An active evidence storage profile must be selected before uploading files.");

    public static readonly Error NotFound =
        Error.NotFound("EvidenceFile.NotFound", "The evidence file was not found.");

    public static readonly Error OwnerMismatch =
        Error.Conflict("EvidenceFile.OwnerMismatch", "The evidence file belongs to another owner.");

    public static readonly Error SessionMismatch =
        Error.NotFound("EvidenceFile.NotFound", "The evidence file was not found in this Session.");

    public static readonly Error UploadExpired =
        Error.Conflict("EvidenceFile.UploadExpired", "The upload attempt has expired.");

    public static readonly Error DirectUploadRequired =
        Error.Conflict("EvidenceFile.DirectUploadRequired", "This storage provider requires direct upload using the supplied URL.");

    public static readonly Error ContentUploadRequired =
        Error.Conflict("EvidenceFile.ContentUploadRequired", "Upload content before confirming the file.");

    public static readonly Error MetadataMismatch =
        Error.Conflict("EvidenceFile.MetadataMismatch", "Stored file metadata does not match the upload reservation.");

    public static readonly Error NotReady =
        Error.Conflict("EvidenceFile.NotReady", "The evidence file is not ready for download.");

    public static readonly Error StorageUnavailable =
        Error.Problem("EvidenceFile.StorageUnavailable", "The configured evidence storage provider is unavailable.");

    public static readonly Error InvalidObjectKey =
        Error.Problem(
            "EvidenceFile.InvalidObjectKey",
            "ObjectKey is required and cannot exceed 1024 characters.");

    public static readonly Error InvalidOriginalFileName =
        Error.Problem(
            "EvidenceFile.InvalidOriginalFileName",
            "OriginalFileName is required and cannot exceed 255 characters.");

    public static readonly Error InvalidContentType =
        Error.Problem(
            "EvidenceFile.InvalidContentType",
            "ContentType is required and cannot exceed 255 characters.");

    public static readonly Error ContentTypeNotAllowed =
        Error.Problem(
            "EvidenceFile.ContentTypeNotAllowed",
            "The file content type is not allowed.");

    public static readonly Error InvalidFileSize =
        Error.Problem(
            "EvidenceFile.InvalidFileSize",
            $"FileSizeBytes must be between 1 and {EvidenceFilePolicy.MaximumFileSizeBytes} bytes.");

    public static readonly Error InvalidUploadExpiration =
        Error.Problem(
            "EvidenceFile.InvalidUploadExpiration",
            "UploadExpiresAt must be later than CreatedAt.");

    public static readonly Error InvalidChecksum =
        Error.Problem(
            "EvidenceFile.InvalidChecksum",
            "A valid Base64-encoded SHA-256 checksum is required.");

    public static readonly Error UploadAttemptMismatch =
        Error.Conflict(
            "EvidenceFile.UploadAttemptMismatch",
            "The upload attempt does not match this evidence file.");

    public static readonly Error CannotStartVerification =
        Error.Conflict(
            "EvidenceFile.CannotStartVerification",
            "Only pending uploads can start verification.");

    public static readonly Error CannotMarkReady =
        Error.Conflict(
            "EvidenceFile.CannotMarkReady",
            "Only uploads being verified can be marked ready.");

    public static readonly Error CannotMarkFailed =
        Error.Conflict(
            "EvidenceFile.CannotMarkFailed",
            "Only pending uploads or uploads being verified can be marked failed.");

    public static readonly Error CannotExpire =
        Error.Conflict(
            "EvidenceFile.CannotExpire",
            "Only pending uploads can expire.");

    public static readonly Error CannotCancel =
        Error.Conflict(
            "EvidenceFile.CannotCancel",
            "Only pending uploads can be cancelled.");

    public static readonly Error CannotRenew =
        Error.Conflict("EvidenceFile.CannotRenew", "Only expired or failed uploads can be renewed.");

    public static readonly Error IdempotencyKeyRequired =
        Error.Problem("EvidenceFile.IdempotencyKeyRequired", "An Idempotency-Key header is required.");

    public static readonly Error IdempotencyKeyConflict =
        Error.Conflict("EvidenceFile.IdempotencyKeyConflict", "The idempotency key was already used with different initialization data.");

    public static readonly Error InvalidStatusChangeTime =
        Error.Problem(
            "EvidenceFile.InvalidStatusChangeTime",
            "A status change cannot occur before the upload was created.");

    public static readonly Error InvalidUploadedAt =
        Error.Problem(
            "EvidenceFile.InvalidUploadedAt",
            "UploadedAt cannot be before the upload was created.");

    public static readonly Error InvalidVersionId =
        Error.Problem(
            "EvidenceFile.InvalidVersionId",
            $"VersionId cannot exceed {EvidenceFile.MaximumVersionIdLength} characters.");

    public static readonly Error InvalidFailureReason =
        Error.Problem(
            "EvidenceFile.InvalidFailureReason",
            $"FailureReason is required and cannot exceed {EvidenceFile.MaximumFailureReasonLength} characters.");

    public static readonly Error UploadNotExpired =
        Error.Conflict(
            "EvidenceFile.UploadNotExpired",
            "The upload cannot expire before UploadExpiresAt.");

    public static readonly Error EvidenceItemRemoved =
        Error.Conflict(
            "EvidenceFile.EvidenceItemRemoved",
            "A file cannot be attached to a removed evidence item.");

    public static readonly Error EvidenceItemTypeMismatch =
        Error.Conflict(
            "EvidenceFile.EvidenceItemTypeMismatch",
            "Files can only be attached to FileReference evidence items.");

    public static readonly Error AlreadyAttached =
        Error.Conflict(
            "EvidenceFile.AlreadyAttached",
            "The evidence item already has a file. File replacement is not supported.");

    public static readonly Error NotAttached =
        Error.Conflict(
            "EvidenceFile.NotAttached",
            "The evidence item does not have an initialized file upload.");
}
