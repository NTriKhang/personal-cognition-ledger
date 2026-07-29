using Common.Application.Messaging;

namespace PCL.Modules.TaskPlanning.Application.Tasks.PlanTask
{
    public sealed record PlanTaskCommand(
        Guid TaskId,
        Guid OwnerId,
        DateTimeOffset PlannedAt) : ICommand;
}

