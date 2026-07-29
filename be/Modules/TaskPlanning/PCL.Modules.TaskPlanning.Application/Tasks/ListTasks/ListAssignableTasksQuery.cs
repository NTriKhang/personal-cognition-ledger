using Common.Application.Messaging;

namespace PCL.Modules.TaskPlanning.Application.Tasks.ListTasks
{
    public sealed record ListAssignableTasksQuery(Guid OwnerId)
        : IQuery<IReadOnlyCollection<TaskSummaryReadModel>>;
}

