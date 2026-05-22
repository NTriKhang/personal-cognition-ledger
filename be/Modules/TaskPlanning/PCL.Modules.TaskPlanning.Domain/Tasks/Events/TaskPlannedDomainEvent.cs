using Common.Domain;

namespace PCL.Modules.TaskPlanning.Domain.Tasks.Events
{
    public sealed class TaskPlannedDomainEvent(TaskId taskId, DateTimeOffset plannedAt) : DomainEvent
    {
        public TaskId TaskId { get; init; } = taskId;
        public DateTimeOffset PlannedAt { get; init; } = plannedAt;
    }
}

