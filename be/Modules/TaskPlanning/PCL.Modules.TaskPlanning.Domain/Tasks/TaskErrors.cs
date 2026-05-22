using Common.Domain;

namespace PCL.Modules.TaskPlanning.Domain.Tasks
{
    public static class TaskErrors
    {
        public static Error NotFound(TaskId taskId) =>
            Error.NotFound("Task.NotFound", $"The task with the identifier {taskId.Value} was not found.");

        public static readonly Error InvalidId =
            Error.Problem(
                "Task.InvalidId",
                "TaskId cannot be empty.");

        public static readonly Error InvalidOwnerId =
            Error.Problem(
                "Task.InvalidOwnerId",
                "OwnerId cannot be empty.");

        public static readonly Error InvalidTitle =
            Error.Problem(
                "Task.InvalidTitle",
                "The task title is required.");

        public static readonly Error InvalidCompletionTime =
            Error.Problem(
                "Task.InvalidCompletionTime",
                "CompletedAt cannot be before the task was created.");

        public static readonly Error InvalidCancellationTime =
            Error.Problem(
                "Task.InvalidCancellationTime",
                "CancelledAt cannot be before the task was created.");

        public static readonly Error AlreadyTerminal =
            Error.Conflict(
                "Task.AlreadyTerminal",
                "Completed and cancelled tasks cannot be changed.");

        public static readonly Error CannotPlan =
            Error.Conflict(
                "Task.CannotPlan",
                "Only draft tasks can be planned.");

        public static readonly Error CannotActivate =
            Error.Conflict(
                "Task.CannotActivate",
                "Only planned tasks can be activated.");

        public static readonly Error CannotDefer =
            Error.Conflict(
                "Task.CannotDefer",
                "Only active tasks can be deferred.");

        public static readonly Error CannotComplete =
            Error.Conflict(
                "Task.CannotComplete",
                "Only planned or active tasks can be completed.");

        public static readonly Error CannotCancel =
            Error.Conflict(
                "Task.CannotCancel",
                "Only draft, planned, or active tasks can be cancelled.");
    }
}

