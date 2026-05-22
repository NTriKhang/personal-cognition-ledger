namespace PCL.Modules.TaskPlanning.Application.Tasks.ListTasks
{
    public sealed class TaskSummaryReadModel
    {
        public Guid Id { get; init; }
        public int Code { get; init; }
        public Guid OwnerId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string? Category { get; init; }
        public string Priority { get; init; } = string.Empty;
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? UpdatedAt { get; init; }
        public DateTimeOffset? CompletedAt { get; init; }
        public DateTimeOffset? CancelledAt { get; init; }
    }
}
