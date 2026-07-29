using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.TaskPlanning.Application.Abstractions.Data;
using PCL.Modules.TaskPlanning.Application.Repositories;
using PCL.Modules.TaskPlanning.Domain.Tasks;

namespace PCL.Modules.TaskPlanning.Application.Tasks.CompleteTask
{
    internal sealed class CompleteTaskCommandHandler(
        ITaskRepository repository,
        IUnitOfWork unitOfWork) : ICommandHandler<CompleteTaskCommand>
    {
        public async Task<Result> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
        {
            TaskId taskId = TaskId.From(request.TaskId);
            Domain.Tasks.Task? task = await repository.GetAsync(taskId, cancellationToken);

            if (task is null || task.OwnerId != request.OwnerId)
                return Result.Failure(TaskErrors.NotFound(taskId));

            Result result = task.Complete(request.CompletedAt, request.CompletionNote);

            if (result.IsFailure)
                return result;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

