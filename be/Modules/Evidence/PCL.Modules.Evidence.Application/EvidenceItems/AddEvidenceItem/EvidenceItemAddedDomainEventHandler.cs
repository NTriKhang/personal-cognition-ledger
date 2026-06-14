using Common.Application.EventBus;
using Common.Application.Messaging;
using PCL.Modules.Evidence.Contracts.EvidenceItems;
using PCL.Modules.Evidence.Domain.EvidenceItems.Events;

namespace PCL.Modules.Evidence.Application.EvidenceItems.AddEvidenceItem;

public sealed class EvidenceItemAddedDomainEventHandler(IEventBus eventBus)
    : DomainEventHandler<EvidenceItemAddedDomainEvent>
{
    public override async Task Handle(
        EvidenceItemAddedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(
            new EvidenceItemAddedIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.OccurredOnUtc,
                domainEvent.EvidenceItemId.Value,
                domainEvent.SessionId,
                domainEvent.OwnerId,
                domainEvent.Type.ToString(),
                domainEvent.Content,
                domainEvent.AddedAt),
            cancellationToken);
    }
}
