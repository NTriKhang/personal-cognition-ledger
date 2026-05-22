namespace PCL.Modules.TaskPlanning.Application.Tasks.ListTasks
{
    public sealed class TaskSessionAssignmentReadModel
    {
        public Guid TaskId { get; init; }
        public Guid SessionId { get; init; }
        public DateTimeOffset AssignedAt { get; init; }
    }
}

