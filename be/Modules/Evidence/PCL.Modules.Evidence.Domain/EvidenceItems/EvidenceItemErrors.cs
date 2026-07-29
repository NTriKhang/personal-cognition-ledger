using Common.Domain;

namespace PCL.Modules.Evidence.Domain.EvidenceItems;

public static class EvidenceItemErrors
{
    public const int MaximumRemovalReasonLength = 1000;

    public static Error NotFound(EvidenceItemId evidenceItemId) =>
        Error.NotFound(
            "EvidenceItem.NotFound",
            $"The evidence item with the identifier {evidenceItemId.Value} was not found.");

    public static readonly Error InvalidSessionId =
        Error.Problem(
            "EvidenceItem.InvalidSessionId",
            "SessionId cannot be empty.");

    public static readonly Error InvalidOwnerId =
        Error.Problem(
            "EvidenceItem.InvalidOwnerId",
            "OwnerId cannot be empty.");

    public static readonly Error InvalidContent =
        Error.Problem(
            "EvidenceItem.InvalidContent",
            "Evidence content is required and must be valid for its type.");

    public static readonly Error InvalidType =
        Error.Problem(
            "EvidenceItem.InvalidType",
            "The evidence type is not supported.");

    public static readonly Error InvalidLink =
        Error.Problem(
            "EvidenceItem.InvalidLink",
            "Link evidence must contain an absolute HTTP or HTTPS URL.");

    public static readonly Error InvalidFileReferenceCaption =
        Error.Problem(
            "EvidenceItem.InvalidFileReferenceCaption",
            $"A file evidence caption cannot exceed {EvidenceItem.MaximumReferenceLength} characters.");

    public static readonly Error FileReferenceRequiresUploadInitialization =
        Error.Conflict(
            "EvidenceItem.FileReferenceRequiresUploadInitialization",
            "File evidence must be created through the file upload initialization workflow.");

    public static readonly Error InvalidRemovedBy =
        Error.Problem(
            "EvidenceItem.InvalidRemovedBy",
            "RemovedBy cannot be empty.");

    public static readonly Error OwnerMismatch =
        Error.Conflict(
            "EvidenceItem.OwnerMismatch",
            "Only the evidence owner can remove the evidence item.");

    public static readonly Error InvalidRemovalTime =
        Error.Problem(
            "EvidenceItem.InvalidRemovalTime",
            "RemovedAt cannot be before the evidence item was added.");

    public static readonly Error InvalidRemovalReason =
        Error.Problem(
            "EvidenceItem.InvalidRemovalReason",
            $"RemovalReason cannot exceed {MaximumRemovalReasonLength} characters.");
}
