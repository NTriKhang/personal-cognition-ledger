using Common.Application.EventBus;
using Common.Application.Messaging;
using PCL.Modules.Session.Contracts.LSessions;
using PCL.Modules.Session.Domain.LSessions.Events;

namespace PCL.Modules.Session.Application.LSessions.EndLearningSession;

public sealed class LSessionStoppedDomainEventHandler(IEventBus eventBus)
    : DomainEventHandler<LSessionStoppedDomainEvent>
{
    public override async Task Handle(
        LSessionStoppedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(
            new SessionStoppedIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.OccurredOnUtc,
                domainEvent.SessionId,
                domainEvent.StoppedAt),
            cancellationToken);
    }
}
