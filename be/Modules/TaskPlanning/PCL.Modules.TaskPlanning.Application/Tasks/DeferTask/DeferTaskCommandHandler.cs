using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.TaskPlanning.Application.Abstractions.Data;
using PCL.Modules.TaskPlanning.Application.Repositories;
using PCL.Modules.TaskPlanning.Domain.Tasks;

namespace PCL.Modules.TaskPlanning.Application.Tasks.DeferTask
{
    internal sealed class DeferTaskCommandHandler(
        ITaskRepository repository,
        IUnitOfWork unitOfWork) : ICommandHandler<DeferTaskCommand>
    {
        public async Task<Result> Handle(DeferTaskCommand request, CancellationToken cancellationToken)
        {
            TaskId taskId = TaskId.From(request.TaskId);
            Domain.Tasks.Task? task = await repository.GetAsync(taskId, cancellationToken);

            if (task is null || task.OwnerId != request.OwnerId)
                return Result.Failure(TaskErrors.NotFound(taskId));

            Result result = task.Defer(request.DeferredAt);

            if (result.IsFailure)
                return result;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

