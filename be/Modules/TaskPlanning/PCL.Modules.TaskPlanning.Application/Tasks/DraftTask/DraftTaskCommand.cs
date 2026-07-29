using Common.Application.Messaging;

namespace PCL.Modules.TaskPlanning.Application.Tasks.DraftTask
{
    public sealed record DraftTaskCommand(
        Guid OwnerId,
        string Title,
        string? Description,
        DateTimeOffset CreatedAt) : ICommand<Guid>;
}

