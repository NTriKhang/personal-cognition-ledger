using Common.Domain;

namespace PCL.Modules.TaskPlanning.Domain.Tasks.Events
{
    public sealed class TaskRefinedDomainEvent(TaskId taskId, string title)
        : DomainEvent
    {
        public TaskId TaskId { get; init; } = taskId;
        public string Title { get; init; } = title;
    }
}
