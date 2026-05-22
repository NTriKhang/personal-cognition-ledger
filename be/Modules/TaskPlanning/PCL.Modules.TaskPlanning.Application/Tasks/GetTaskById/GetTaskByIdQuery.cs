using Common.Application.Messaging;

namespace PCL.Modules.TaskPlanning.Application.Tasks.GetTaskById
{
    public sealed record GetTaskByIdQuery(Guid TaskId, Guid OwnerId) : IQuery<TaskDetailReadModel>;
}

