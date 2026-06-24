using Common.Domain;

namespace PCL.Modules.TaskPlanning.Contracts.Tasks;

public static class TaskAssignmentEligibilityErrors
{
    public static Error NotFound(Guid taskId) =>
        Error.NotFound(
            "TaskAssignmentEligibility.TaskNotFound",
            $"The task with the identifier {taskId} was not found.");

    public static readonly Error OwnerMismatch =
        Error.Conflict(
            "TaskAssignmentEligibility.OwnerMismatch",
            "The task does not belong to the session owner.");

    public static readonly Error NotAssignable =
        Error.Conflict(
            "TaskAssignmentEligibility.NotAssignable",
            "Only planned or active tasks can be assigned to a session.");
}
