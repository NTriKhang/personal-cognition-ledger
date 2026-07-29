using Common.Application.Messaging;
using PCL.Modules.TaskPlanning.Domain.Tasks;

namespace PCL.Modules.TaskPlanning.Application.Tasks.ListTasks
{
    public sealed record ListTasksByCategoryQuery(
        Guid OwnerId,
        TaskCategory Category) : IQuery<IReadOnlyCollection<TaskSummaryReadModel>>;
}

