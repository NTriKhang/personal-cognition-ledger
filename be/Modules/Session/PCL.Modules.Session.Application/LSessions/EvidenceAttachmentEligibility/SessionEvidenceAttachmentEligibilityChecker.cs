using Common.Domain;
using PCL.Modules.Session.Application.Repositories;
using PCL.Modules.Session.Contracts.LSessions;
using PCL.Modules.Session.Domain.LSessions;

namespace PCL.Modules.Session.Application.LSessions.EvidenceAttachmentEligibility;

public sealed class SessionEvidenceAttachmentEligibilityChecker(ILSessionRepository repository)
    : ISessionEvidenceAttachmentEligibilityChecker
{
    public async Task<Result> CheckAsync(
        Guid sessionId,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        LSession? session = await repository.GetAsync(sessionId);

        if (session is null)
            return Result.Failure(SessionEvidenceAttachmentEligibilityErrors.NotFound(sessionId));

        if (session.OwnerId != ownerId)
            return Result.Failure(SessionEvidenceAttachmentEligibilityErrors.OwnerMismatch);

        if (session.Status != LSessionStatus.Active)
            return Result.Failure(SessionEvidenceAttachmentEligibilityErrors.NotActive);

        return Result.Success();
    }
}
