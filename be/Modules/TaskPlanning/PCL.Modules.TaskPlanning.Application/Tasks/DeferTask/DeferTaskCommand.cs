using Common.Application.Messaging;

namespace PCL.Modules.TaskPlanning.Application.Tasks.DeferTask
{
    public sealed record DeferTaskCommand(
        Guid TaskId,
        Guid OwnerId,
        DateTimeOffset DeferredAt) : ICommand;
}

