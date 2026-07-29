using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Domain.EvidenceItems;

public sealed class EvidenceFileUploadAttempt
{
    public EvidenceFileUploadAttemptId Id { get; private set; }
    public EvidenceItemId EvidenceItemId { get; private set; }
    public StorageProfileId StorageProfileId { get; private set; }
    public string ObjectKey { get; private set; } = string.Empty;
    public EvidenceFileUploadStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset StatusChangedAt { get; private set; }
    public DateTimeOffset? PhysicalDeletedAt { get; private set; }
    public int CleanupAttempts { get; private set; }
    public DateTimeOffset? CleanupNextAttemptAt { get; private set; }

    private EvidenceFileUploadAttempt() { }

    internal static EvidenceFileUploadAttempt Archive(EvidenceFile file) => new()
    {
        Id = file.UploadAttemptId,
        EvidenceItemId = file.EvidenceItemId,
        StorageProfileId = file.StorageProfileId,
        ObjectKey = file.ObjectKey,
        Status = file.UploadStatus,
        CreatedAt = file.CreatedAt,
        ExpiresAt = file.UploadExpiresAt,
        StatusChangedAt = file.StatusChangedAt
    };

    public void MarkPhysicallyDeleted(DateTimeOffset deletedAt) => PhysicalDeletedAt ??= deletedAt;
    public void MarkCleanupFailed(DateTimeOffset failedAt)
    {
        CleanupAttempts++;
        CleanupNextAttemptAt = failedAt.AddMinutes(Math.Min(Math.Pow(2, CleanupAttempts - 1), 60));
    }
}
