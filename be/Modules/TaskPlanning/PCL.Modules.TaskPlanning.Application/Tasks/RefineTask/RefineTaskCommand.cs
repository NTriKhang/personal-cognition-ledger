using Common.Application.Messaging;

namespace PCL.Modules.TaskPlanning.Application.Tasks.RefineTask
{
    public sealed record RefineTaskCommand(
        Guid TaskId,
        Guid OwnerId,
        string Title,
        string? Description,
        DateTimeOffset UpdatedAt) : ICommand;
}

