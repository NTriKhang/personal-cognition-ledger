using Common.Domain;

namespace PCL.Modules.Session.Contracts.LSessions;

public static class SessionEvidenceAttachmentEligibilityErrors
{
    public static Error NotFound(Guid sessionId) =>
        Error.NotFound(
            "SessionEvidenceAttachmentEligibility.SessionNotFound",
            $"The session with the identifier {sessionId} was not found.");

    public static readonly Error OwnerMismatch =
        Error.Conflict(
            "SessionEvidenceAttachmentEligibility.OwnerMismatch",
            "The session does not belong to the evidence owner.");

    public static readonly Error NotActive =
        Error.Conflict(
            "SessionEvidenceAttachmentEligibility.NotActive",
            "Evidence can only be attached to an active session.");
}
