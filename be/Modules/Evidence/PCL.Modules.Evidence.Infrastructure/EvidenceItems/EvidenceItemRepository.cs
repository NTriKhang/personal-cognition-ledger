using Microsoft.EntityFrameworkCore;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Domain.EvidenceItems;
using PCL.Modules.Evidence.Infrastructure.Database;

namespace PCL.Modules.Evidence.Infrastructure.EvidenceItems;

public sealed class EvidenceItemRepository(EvidenceDbContext context) : IEvidenceItemRepository
{
    public void Add(EvidenceItem evidenceItem)
    {
        context.EvidenceItems.Add(evidenceItem);
    }

    public async Task<EvidenceItem?> GetAsync(
        EvidenceItemId evidenceItemId,
        CancellationToken cancellationToken = default)
    {
        return await context.EvidenceItems
            .Include(evidenceItem => evidenceItem.File)
            .SingleOrDefaultAsync(
                evidenceItem => evidenceItem.Id == evidenceItemId,
                cancellationToken);
    }
}
