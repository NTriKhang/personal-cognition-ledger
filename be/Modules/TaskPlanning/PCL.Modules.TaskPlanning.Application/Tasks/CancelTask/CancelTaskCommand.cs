using Common.Application.Messaging;

namespace PCL.Modules.TaskPlanning.Application.Tasks.CancelTask
{
    public sealed record CancelTaskCommand(
        Guid TaskId,
        Guid OwnerId,
        DateTimeOffset CancelledAt,
        string? CancellationReason) : ICommand;
}

