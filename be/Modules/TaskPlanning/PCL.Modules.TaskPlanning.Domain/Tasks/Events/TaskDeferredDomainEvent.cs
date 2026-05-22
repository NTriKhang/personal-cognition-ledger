using Common.Domain;

namespace PCL.Modules.TaskPlanning.Domain.Tasks.Events
{
    public sealed class TaskDeferredDomainEvent(TaskId taskId, DateTimeOffset deferredAt) : DomainEvent
    {
        public TaskId TaskId { get; init; } = taskId;
        public DateTimeOffset DeferredAt { get; init; } = deferredAt;
    }
}

