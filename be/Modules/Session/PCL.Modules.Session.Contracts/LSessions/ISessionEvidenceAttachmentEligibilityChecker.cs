using Common.Domain;

namespace PCL.Modules.Session.Contracts.LSessions;

public interface ISessionEvidenceAttachmentEligibilityChecker
{
    Task<Result> CheckAsync(
        Guid sessionId,
        Guid ownerId,
        CancellationToken cancellationToken = default);
}
