using Common.Application.EventBus;

namespace PCL.Modules.Evidence.Contracts.EvidenceItems;

public sealed class EvidenceItemAddedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid evidenceItemId,
    Guid sessionId,
    Guid ownerId,
    string type,
    string content,
    DateTimeOffset addedAt)
    : IntegrationEvent(id, occurredOnUtc)
{
    public Guid EvidenceItemId { get; init; } = evidenceItemId;
    public Guid SessionId { get; init; } = sessionId;
    public Guid OwnerId { get; init; } = ownerId;
    public string Type { get; init; } = type;
    public string Content { get; init; } = content;
    public DateTimeOffset AddedAt { get; init; } = addedAt;
}
