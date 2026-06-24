using Common.Domain;

namespace PCL.Modules.TaskPlanning.Contracts.Tasks;

public interface ITaskAssignmentEligibilityChecker
{
    Task<Result> CheckAsync(
        Guid taskId,
        Guid ownerId,
        CancellationToken cancellationToken = default);
}
