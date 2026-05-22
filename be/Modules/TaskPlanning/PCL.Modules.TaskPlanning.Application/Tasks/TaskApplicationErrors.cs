using Common.Domain;

namespace PCL.Modules.TaskPlanning.Application.Tasks
{
    internal static class TaskApplicationErrors
    {
        public static readonly Error CancellationReasonRequired =
            Error.Problem(
                "Task.CancellationReasonRequired",
                "Cancellation reason is required when cancelling an active task.");
    }
}

