using Common.Application.Messaging;

namespace PCL.Modules.Evidence.Application.EvidenceItems.ListEvidenceItemsBySession;

public sealed record ListEvidenceItemsBySessionQuery(
    Guid SessionId,
    Guid OwnerId,
    bool IncludeRemoved = false)
    : IQuery<IReadOnlyCollection<EvidenceItemReadModel>>;
