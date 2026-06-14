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
