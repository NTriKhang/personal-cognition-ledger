using Common.Domain;

namespace PCL.Modules.TaskPlanning.Domain.Tasks.Events
{
    public sealed class TaskCategoryChangedDomainEvent(TaskId taskId, TaskCategory category) : DomainEvent
    {
        public TaskId TaskId { get; init; } = taskId;
        public TaskCategory Category { get; init; } = category;
    }
}

