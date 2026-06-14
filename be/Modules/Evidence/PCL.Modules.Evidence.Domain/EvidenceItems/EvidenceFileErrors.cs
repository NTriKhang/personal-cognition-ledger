using Common.Domain;

namespace PCL.Modules.Evidence.Domain.EvidenceItems;

public static class EvidenceFileErrors
{
    public static readonly Error InvalidEvidenceItemId =
        Error.Problem(
            "EvidenceFile.InvalidEvidenceItemId",
            "EvidenceItemId cannot be empty.");

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

    public static readonly Error InvalidFileSize =
        Error.Problem(
            "EvidenceFile.InvalidFileSize",
            "FileSizeBytes cannot be negative.");

    public static readonly Error InvalidUploadExpiration =
        Error.Problem(
            "EvidenceFile.InvalidUploadExpiration",
            "UploadExpiresAt must be later than CreatedAt.");

    public static readonly Error InvalidChecksum =
        Error.Problem(
            "EvidenceFile.InvalidChecksum",
            "ChecksumAlgorithm and ChecksumValue must either both be supplied or both be omitted.");

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
            "The evidence item already has a file.");
}
