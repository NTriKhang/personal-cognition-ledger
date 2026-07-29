using Common.Domain;

namespace PCL.Modules.TaskPlanning.Domain.Tasks.Events
{
    public sealed class TaskCancelledDomainEvent(TaskId taskId, DateTimeOffset cancelledAt, string? reason)
        : DomainEvent
    {
        public TaskId TaskId { get; init; } = taskId;
        public DateTimeOffset CancelledAt { get; init; } = cancelledAt;
        public string? Reason { get; init; } = reason;
    }
}
