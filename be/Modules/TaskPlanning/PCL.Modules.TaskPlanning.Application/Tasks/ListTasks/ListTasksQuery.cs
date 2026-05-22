using Common.Application.Messaging;
using PCL.Modules.TaskPlanning.Domain.Tasks;
using PlanningTaskStatus = PCL.Modules.TaskPlanning.Domain.Tasks.TaskStatus;

namespace PCL.Modules.TaskPlanning.Application.Tasks.ListTasks
{
    public sealed record ListTasksQuery(
        Guid OwnerId,
        PlanningTaskStatus? Status,
        TaskCategory? Category,
        TaskPriority? Priority,
        string? Search,
        DateTimeOffset? CreatedFrom,
        DateTimeOffset? CreatedTo,
        bool AssignableOnly = false) : IQuery<IReadOnlyCollection<TaskSummaryReadModel>>;
}
