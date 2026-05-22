namespace PCL.Modules.TaskPlanning.Domain.Tasks
{
    public readonly record struct TaskId(Guid Value)
    {
        public static TaskId New() => new(Guid.NewGuid());

        public static TaskId From(Guid value) => new(value);
    }
}

