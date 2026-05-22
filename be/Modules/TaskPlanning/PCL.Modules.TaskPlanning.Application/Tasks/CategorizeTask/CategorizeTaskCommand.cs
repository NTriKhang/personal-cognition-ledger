using Common.Application.Messaging;
using PCL.Modules.TaskPlanning.Domain.Tasks;

namespace PCL.Modules.TaskPlanning.Application.Tasks.CategorizeTask
{
    public sealed record CategorizeTaskCommand(
        Guid TaskId,
        Guid OwnerId,
        TaskCategory Category,
        DateTimeOffset UpdatedAt) : ICommand;
}

