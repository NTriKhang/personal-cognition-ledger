namespace PCL.Modules.Evidence.Domain.EvidenceItems;

public sealed class EvidenceFileInitialization
{
    public Guid OwnerId { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string RequestHash { get; private set; } = string.Empty;
    public EvidenceItemId EvidenceItemId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private EvidenceFileInitialization() { }

    public static EvidenceFileInitialization Create(Guid ownerId, string key, string hash, EvidenceItemId itemId, DateTimeOffset createdAt) =>
        new() { OwnerId = ownerId, IdempotencyKey = key, RequestHash = hash, EvidenceItemId = itemId, CreatedAt = createdAt };
}
