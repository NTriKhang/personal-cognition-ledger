using Common.Domain;
using PCL.Modules.TaskPlanning.Application.Repositories;
using PCL.Modules.TaskPlanning.Domain.Tasks;
using PlanningTaskStatus = PCL.Modules.TaskPlanning.Domain.Tasks.TaskStatus;
using PlanningTask = PCL.Modules.TaskPlanning.Domain.Tasks.Task;
using PCL.Modules.TaskPlanning.Contracts.Tasks;

namespace PCL.Modules.TaskPlanning.Application.Tasks.AssignmentEligibility;

public sealed class TaskAssignmentEligibilityChecker(ITaskRepository taskRepository)
    : ITaskAssignmentEligibilityChecker
{
    public async Task<Result> CheckAsync(
        Guid taskId,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        PlanningTask? task = await taskRepository.GetAsync(TaskId.From(taskId), cancellationToken);

        if (task is null)
        {
            return Result.Failure(TaskAssignmentEligibilityErrors.NotFound(taskId));
        }

        if (task.OwnerId != ownerId)
        {
            return Result.Failure(TaskAssignmentEligibilityErrors.OwnerMismatch);
        }

        if (task.Status is not PlanningTaskStatus.Planned and not PlanningTaskStatus.Active)
        {
            return Result.Failure(TaskAssignmentEligibilityErrors.NotAssignable);
        }

        return Result.Success();
    }
}
