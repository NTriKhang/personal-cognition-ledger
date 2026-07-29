using Common.Application.Messaging;
using PCL.Modules.TaskPlanning.Domain.Tasks;

namespace PCL.Modules.TaskPlanning.Application.Tasks.PrioritizeTask
{
    public sealed record PrioritizeTaskCommand(
        Guid TaskId,
        Guid OwnerId,
        TaskPriority Priority,
        DateTimeOffset UpdatedAt) : ICommand;
}

