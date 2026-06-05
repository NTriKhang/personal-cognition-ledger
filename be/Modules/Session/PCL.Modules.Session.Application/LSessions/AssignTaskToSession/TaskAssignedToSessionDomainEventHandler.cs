using Common.Application.EventBus;
using Common.Application.Messaging;
using PCL.Modules.Session.Contracts.LSessions;
using PCL.Modules.Session.Domain.LSessions.Events;

namespace PCL.Modules.Session.Application.LSessions.AssignTaskToSession;

public sealed class TaskAssignedToSessionDomainEventHandler(IEventBus eventBus)
    : DomainEventHandler<TaskAssignedToSessionDomainEvent>
{
    public override async Task Handle(
        TaskAssignedToSessionDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(
            new TaskAssignedToSessionIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.OccurredOnUtc,
                domainEvent.SessionId,
                domainEvent.OwnerId,
                domainEvent.TaskId,
                domainEvent.AssignedAt),
            cancellationToken);
    }
}
