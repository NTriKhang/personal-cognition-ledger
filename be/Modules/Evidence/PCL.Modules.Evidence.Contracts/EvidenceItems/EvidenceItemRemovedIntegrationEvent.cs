using Common.Application.EventBus;

namespace PCL.Modules.Evidence.Contracts.EvidenceItems;

public sealed class EvidenceItemRemovedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid evidenceItemId,
    Guid sessionId,
    Guid ownerId,
    Guid removedBy,
    DateTimeOffset removedAt,
    string? removalReason)
    : IntegrationEvent(id, occurredOnUtc)
{
    public Guid EvidenceItemId { get; init; } = evidenceItemId;
    public Guid SessionId { get; init; } = sessionId;
    public Guid OwnerId { get; init; } = ownerId;
    public Guid RemovedBy { get; init; } = removedBy;
    public DateTimeOffset RemovedAt { get; init; } = removedAt;
    public string? RemovalReason { get; init; } = removalReason;
}
