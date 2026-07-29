using Microsoft.EntityFrameworkCore;
using Common.Domain;
using Microsoft.Extensions.Logging;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Domain.EvidenceItems;
using PCL.Modules.Evidence.Infrastructure.Database;
using Quartz;

namespace PCL.Modules.Evidence.Infrastructure.EvidenceItems;

[DisallowConcurrentExecution]
internal sealed class ReconcileEvidenceFileUploadsJob(EvidenceDbContext db, IStorageProfileRepository profiles,
    IEvidenceFileStorageResolver resolver, ILogger<ReconcileEvidenceFileUploadsJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        CancellationToken ct = context.CancellationToken;
        DateTimeOffset now = DateTimeOffset.UtcNow;
        List<EvidenceItem> items = await db.EvidenceItems.Include(x => x.File)
            .Where(x => x.File != null && x.File.UploadStatus == EvidenceFileUploadStatus.Pending && x.File.UploadExpiresAt <= now)
            .OrderBy(x => x.File!.UploadExpiresAt).Take(100).ToListAsync(ct);
        foreach (EvidenceItem item in items)
        {
            EvidenceFile file = item.File!;
            try
            {
                var profile = await profiles.GetAsync(file.StorageProfileId, ct);
                if (profile is null) continue;
                StoredFileMetadata? metadata = await resolver.Resolve(profile.ProviderType).GetMetadataAsync(profile, file.ObjectKey, ct);
                if (metadata is not null && metadata.UploadedAt <= file.UploadExpiresAt && metadata.Size == file.FileSizeBytes &&
                    (metadata.ContentType == "application/octet-stream" || string.Equals(metadata.ContentType, file.ContentType, StringComparison.OrdinalIgnoreCase)) &&
                    string.Equals(metadata.Checksum, file.ChecksumValue, StringComparison.Ordinal))
                {
                    Result started = item.StartFileVerification(file.UploadAttemptId, now);
                    if (started.IsSuccess) item.MarkFileReady(file.UploadAttemptId, metadata.UploadedAt, now, metadata.VersionId);
                }
                else item.ExpireFileUpload(file.UploadAttemptId, now);
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException) { db.ChangeTracker.Clear(); break; }
            catch (Exception ex) { logger.LogWarning(ex, "Evidence upload reconciliation will retry after a provider failure."); db.ChangeTracker.Clear(); break; }
        }
    }
}
