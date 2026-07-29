using Common.Domain;

namespace PCL.Modules.Evidence.Domain.EvidenceItems.Events;

public sealed class EvidenceItemRemovedDomainEvent(
    EvidenceItemId evidenceItemId,
    Guid sessionId,
    Guid ownerId,
    Guid removedBy,
    DateTimeOffset removedAt,
    string? removalReason) : DomainEvent
{
    public EvidenceItemId EvidenceItemId { get; init; } = evidenceItemId;
    public Guid SessionId { get; init; } = sessionId;
    public Guid OwnerId { get; init; } = ownerId;
    public Guid RemovedBy { get; init; } = removedBy;
    public DateTimeOffset RemovedAt { get; init; } = removedAt;
    public string? RemovalReason { get; init; } = removalReason;
}
