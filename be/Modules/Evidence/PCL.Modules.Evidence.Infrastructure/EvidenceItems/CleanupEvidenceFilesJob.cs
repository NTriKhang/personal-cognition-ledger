using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Domain.EvidenceItems;
using PCL.Modules.Evidence.Infrastructure.Database;
using Quartz;

namespace PCL.Modules.Evidence.Infrastructure.EvidenceItems;

[DisallowConcurrentExecution]
internal sealed class CleanupEvidenceFilesJob(EvidenceDbContext db, IStorageProfileRepository profiles,
    IEvidenceFileStorageResolver resolver, ILogger<CleanupEvidenceFilesJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        CancellationToken ct = context.CancellationToken;
        DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddDays(-7);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var history = await db.EvidenceFileUploadAttempts.Where(x => x.PhysicalDeletedAt == null && x.StatusChangedAt <= cutoff &&
                (x.CleanupNextAttemptAt == null || x.CleanupNextAttemptAt <= now))
            .OrderBy(x => x.StatusChangedAt).Take(100).ToListAsync(ct);
        foreach (EvidenceFileUploadAttempt attempt in history)
            await Delete(attempt.StorageProfileId, attempt.ObjectKey, () => attempt.MarkPhysicallyDeleted(now), () => attempt.MarkCleanupFailed(now), ct);

        int remaining = 100 - history.Count;
        if (remaining <= 0) return;
        var current = await db.EvidenceItems.Include(x => x.File).Where(x => x.File != null && x.File.PhysicalDeletedAt == null &&
            (x.File.CleanupNextAttemptAt == null || x.File.CleanupNextAttemptAt <= now) &&
            ((x.RemovedAt != null && x.RemovedAt <= cutoff) ||
             (x.RemovedAt == null && x.File.StatusChangedAt <= cutoff &&
              (x.File.UploadStatus == EvidenceFileUploadStatus.Expired || x.File.UploadStatus == EvidenceFileUploadStatus.Failed || x.File.UploadStatus == EvidenceFileUploadStatus.Cancelled))))
            .OrderBy(x => x.File!.StatusChangedAt).Take(remaining).ToListAsync(ct);
        foreach (EvidenceItem item in current)
            await Delete(item.File!.StorageProfileId, item.File.ObjectKey, () => item.File.MarkPhysicallyDeleted(now), () => item.File.MarkCleanupFailed(now), ct);
    }

    private async Task Delete(Domain.Storage.StorageProfileId profileId, string key, Action mark, Action failed, CancellationToken ct)
    {
        try
        {
            var profile = await profiles.GetAsync(profileId, ct);
            if (profile is null)
            {
                failed();
                await db.SaveChangesAsync(ct);
                return;
            }
            await resolver.Resolve(profile.ProviderType).DeleteAsync(profile, key, ct);
            mark();
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Evidence file cleanup will retry after a provider failure.");
            failed();
            try { await db.SaveChangesAsync(ct); } catch (Exception saveEx) { logger.LogError(saveEx, "Could not persist Evidence cleanup retry state."); db.ChangeTracker.Clear(); }
        }
    }
}
