using Common.Domain;

namespace PCL.Modules.Evidence.Domain.EvidenceItems.Events;

public sealed class EvidenceItemAddedDomainEvent(
    EvidenceItemId evidenceItemId,
    Guid sessionId,
    Guid ownerId,
    EvidenceItemType type,
    string content,
    DateTimeOffset addedAt) : DomainEvent
{
    public EvidenceItemId EvidenceItemId { get; init; } = evidenceItemId;
    public Guid SessionId { get; init; } = sessionId;
    public Guid OwnerId { get; init; } = ownerId;
    public EvidenceItemType Type { get; init; } = type;
    public string Content { get; init; } = content;
    public DateTimeOffset AddedAt { get; init; } = addedAt;
}
