using Common.Application.Messaging;

namespace PCL.Modules.Evidence.Application.EvidenceItems.RemoveEvidenceItem;

public sealed record RemoveEvidenceItemCommand(
    Guid SessionId,
    Guid EvidenceItemId,
    Guid OwnerId,
    DateTimeOffset RemovedAt,
    string? RemovalReason) : ICommand;
