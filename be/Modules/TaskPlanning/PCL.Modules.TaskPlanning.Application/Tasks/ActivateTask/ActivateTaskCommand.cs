using Common.Application.Messaging;

namespace PCL.Modules.TaskPlanning.Application.Tasks.ActivateTask
{
    public sealed record ActivateTaskCommand(
        Guid TaskId,
        Guid OwnerId,
        DateTimeOffset ActivatedAt) : ICommand;
}

