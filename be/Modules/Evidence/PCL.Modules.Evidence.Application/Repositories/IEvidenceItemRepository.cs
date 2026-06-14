using PCL.Modules.Evidence.Domain.EvidenceItems;

namespace PCL.Modules.Evidence.Application.Repositories;

public interface IEvidenceItemRepository
{
    void Add(EvidenceItem evidenceItem);

    Task<EvidenceItem?> GetAsync(
        EvidenceItemId evidenceItemId,
        CancellationToken cancellationToken = default);
}
