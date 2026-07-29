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

    public void Add(EvidenceFileUploadAttempt attempt) => context.EvidenceFileUploadAttempts.Add(attempt);
    public void Add(EvidenceFileInitialization initialization) => context.EvidenceFileInitializations.Add(initialization);
    public Task<EvidenceFileInitialization?> GetInitializationAsync(Guid ownerId, string idempotencyKey, CancellationToken cancellationToken = default) =>
        context.EvidenceFileInitializations.SingleOrDefaultAsync(x => x.OwnerId == ownerId && x.IdempotencyKey == idempotencyKey, cancellationToken);
    public Task<EvidenceFileUploadAttempt?> GetAttemptAsync(EvidenceFileUploadAttemptId attemptId, CancellationToken cancellationToken = default) =>
        context.EvidenceFileUploadAttempts.SingleOrDefaultAsync(x => x.Id == attemptId, cancellationToken);

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
