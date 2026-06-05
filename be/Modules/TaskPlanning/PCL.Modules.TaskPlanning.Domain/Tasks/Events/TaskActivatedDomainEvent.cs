using Common.Domain;

namespace PCL.Modules.TaskPlanning.Domain.Tasks.Events
{
    public sealed class TaskActivatedDomainEvent(TaskId taskId, DateTimeOffset activatedAt)
        : DomainEvent, ITaskProjectionAffectingDomainEvent
    {
        public TaskId TaskId { get; init; } = taskId;
        public DateTimeOffset ActivatedAt { get; init; } = activatedAt;
    }
}
