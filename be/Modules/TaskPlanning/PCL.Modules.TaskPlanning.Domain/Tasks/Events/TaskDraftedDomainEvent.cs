using Common.Domain;

namespace PCL.Modules.TaskPlanning.Domain.Tasks.Events
{
    public sealed class TaskDraftedDomainEvent(TaskId taskId, Guid ownerId, string title) : DomainEvent
    {
        public TaskId TaskId { get; init; } = taskId;
        public Guid OwnerId { get; init; } = ownerId;
        public string Title { get; init; } = title;
    }
}

