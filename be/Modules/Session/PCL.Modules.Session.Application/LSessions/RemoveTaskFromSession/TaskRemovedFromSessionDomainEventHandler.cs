using Common.Application.EventBus;
using Common.Application.Messaging;
using PCL.Modules.Session.Contracts.LSessions;
using PCL.Modules.Session.Domain.LSessions.Events;

namespace PCL.Modules.Session.Application.LSessions.RemoveTaskFromSession;

public sealed class TaskRemovedFromSessionDomainEventHandler(IEventBus eventBus)
    : DomainEventHandler<TaskRemovedFromSessionDomainEvent>
{
    public override async Task Handle(
        TaskRemovedFromSessionDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(
            new TaskRemovedFromSessionIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.OccurredOnUtc,
                domainEvent.SessionId,
                domainEvent.OwnerId,
                domainEvent.TaskId),
            cancellationToken);
    }
}
