using Common.Domain;

namespace PCL.Modules.TaskPlanning.Domain.Tasks.Events
{
    public sealed class TaskPriorityChangedDomainEvent(TaskId taskId, TaskPriority priority) : DomainEvent
    {
        public TaskId TaskId { get; init; } = taskId;
        public TaskPriority Priority { get; init; } = priority;
    }
}

