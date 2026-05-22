using Common.Application.Messaging;

namespace PCL.Modules.TaskPlanning.Application.Tasks.CompleteTask
{
    public sealed record CompleteTaskCommand(
        Guid TaskId,
        Guid OwnerId,
        DateTimeOffset CompletedAt,
        string? CompletionNote) : ICommand;
}

