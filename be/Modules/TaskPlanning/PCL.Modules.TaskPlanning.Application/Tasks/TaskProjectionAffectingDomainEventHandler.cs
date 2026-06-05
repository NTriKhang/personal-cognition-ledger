using Common.Application.EventBus;
using Common.Application.Messaging;
using PCL.Modules.TaskPlanning.Contracts.Tasks;
using PCL.Modules.TaskPlanning.Application.Repositories;
using PCL.Modules.TaskPlanning.Domain.Tasks.Events;
using PlanningTask = PCL.Modules.TaskPlanning.Domain.Tasks.Task;

namespace PCL.Modules.TaskPlanning.Application.Tasks;

public sealed class TaskProjectionAffectingDomainEventHandler(
    ITaskRepository taskRepository,
    IEventBus eventBus)
    : DomainEventHandler<ITaskProjectionAffectingDomainEvent>
{
    public override async System.Threading.Tasks.Task Handle(
        ITaskProjectionAffectingDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        PlanningTask? task = await taskRepository.GetAsync(domainEvent.TaskId, cancellationToken);

        if (task is null)
        {
            return;
        }

        await eventBus.PublishAsync(
            new TaskSnapshotChangedIntegrationEvent(
                Guid.NewGuid(),
                domainEvent.OccurredOnUtc,
                task.Id.Value,
                task.Code,
                task.OwnerId,
                task.Title,
                task.Description,
                task.Status.ToString(),
                task.Category?.ToString(),
                task.Priority.ToString(),
                task.CreatedAt,
                task.UpdatedAt,
                task.PlannedAt,
                task.ActivatedAt,
                task.CompletedAt,
                task.CancelledAt),
            cancellationToken);
    }
}
