using PCL.Modules.Evidence.Domain.EvidenceItems;

namespace PCL.Modules.Evidence.Application.Repositories;

public interface IEvidenceItemRepository
{
    void Add(EvidenceItem evidenceItem);
    void Add(EvidenceFileUploadAttempt attempt);
    void Add(EvidenceFileInitialization initialization);

    Task<EvidenceFileInitialization?> GetInitializationAsync(Guid ownerId, string idempotencyKey, CancellationToken cancellationToken = default);
    Task<EvidenceFileUploadAttempt?> GetAttemptAsync(EvidenceFileUploadAttemptId attemptId, CancellationToken cancellationToken = default);

    Task<EvidenceItem?> GetAsync(
        EvidenceItemId evidenceItemId,
        CancellationToken cancellationToken = default);
}
