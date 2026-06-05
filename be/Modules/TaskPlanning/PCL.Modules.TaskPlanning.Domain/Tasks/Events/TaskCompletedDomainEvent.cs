using Common.Domain;

namespace PCL.Modules.TaskPlanning.Domain.Tasks.Events
{
    public sealed class TaskCompletedDomainEvent(TaskId taskId, DateTimeOffset completedAt)
        : DomainEvent
    {
        public TaskId TaskId { get; init; } = taskId;
        public DateTimeOffset CompletedAt { get; init; } = completedAt;
    }
}
