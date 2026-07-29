using Common.Application.Messaging;
using PCL.Modules.Evidence.Domain.EvidenceItems;

namespace PCL.Modules.Evidence.Application.EvidenceItems.AddEvidenceItem;

public sealed record AddEvidenceItemCommand(
    Guid SessionId,
    Guid OwnerId,
    EvidenceItemType Type,
    string Content,
    DateTimeOffset AddedAt) : ICommand<Guid>;
