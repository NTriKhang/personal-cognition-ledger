using Common.Domain;
using PCL.Modules.Session.Application.Repositories;
using PCL.Modules.Session.Domain.LSessions;
using PCL.Modules.TaskPlanning.Contracts.Tasks;

namespace PCL.Modules.Session.Application.LSessions.AssignTaskToSession;

public sealed class AssignTaskToSessionPolicy(
    ITaskAssignmentEligibilityChecker taskAssignmentEligibilityChecker,
    ILSessionRepository sessionRepository)
    : IAssignTaskToSessionPolicy
{
    public async Task<Result> ValidateAsync(
        LSession session,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        Result eligibilityResult = await taskAssignmentEligibilityChecker.CheckAsync(
            taskId,
            session.OwnerId,
            cancellationToken);

        if (eligibilityResult.IsFailure)
        {
            return eligibilityResult;
        }

        bool taskAssignedToAnotherActiveSession =
            await sessionRepository.IsTaskAssignedToAnotherActiveSessionAsync(
                taskId,
                session.Id,
                session.OwnerId,
                cancellationToken);

        if (taskAssignedToAnotherActiveSession)
        {
            return Result.Failure(LSessionErrors.TaskAlreadyAssignedToAnotherActiveSession);
        }

        return Result.Success();
    }
}
