using Common.Application.EventBus;
using Common.Application.Messaging;
using PCL.Modules.Evidence.Contracts.EvidenceItems;
using PCL.Modules.Evidence.Domain.EvidenceItems.Events;

namespace PCL.Modules.Evidence.Application.EvidenceItems.RemoveEvidenceItem;

public sealed class EvidenceItemRemovedDomainEventHandler(IEventBus eventBus)
    : DomainEventHandler<EvidenceItemRemovedDomainEvent>
{
    public override async Task Handle(
        EvidenceItemRemovedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(
            new EvidenceItemRemovedIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.OccurredOnUtc,
                domainEvent.EvidenceItemId.Value,
                domainEvent.SessionId,
                domainEvent.OwnerId,
                domainEvent.RemovedBy,
                domainEvent.RemovedAt,
                domainEvent.RemovalReason),
            cancellationToken);
    }
}
