using Common.Application.Messaging;
using PCL.Modules.TaskPlanning.Domain.Tasks;
using PlanningTaskStatus = PCL.Modules.TaskPlanning.Domain.Tasks.TaskStatus;

namespace PCL.Modules.TaskPlanning.Application.Tasks.ListTasks
{
    public sealed record ListTasksByStatusQuery(
        Guid OwnerId,
        PlanningTaskStatus Status) : IQuery<IReadOnlyCollection<TaskSummaryReadModel>>;
}
