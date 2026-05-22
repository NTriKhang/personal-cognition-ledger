using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.TaskPlanning.Application.Abstractions.Data;
using PCL.Modules.TaskPlanning.Application.Repositories;
using PCL.Modules.TaskPlanning.Application.Tasks;
using PCL.Modules.TaskPlanning.Domain.Tasks;
using PlanningTaskStatus = PCL.Modules.TaskPlanning.Domain.Tasks.TaskStatus;

namespace PCL.Modules.TaskPlanning.Application.Tasks.CancelTask
{
    internal sealed class CancelTaskCommandHandler(
        ITaskRepository repository,
        IUnitOfWork unitOfWork) : ICommandHandler<CancelTaskCommand>
    {
        public async Task<Result> Handle(CancelTaskCommand request, CancellationToken cancellationToken)
        {
            TaskId taskId = TaskId.From(request.TaskId);
            Domain.Tasks.Task? task = await repository.GetAsync(taskId, cancellationToken);

            if (task is null || task.OwnerId != request.OwnerId)
                return Result.Failure(TaskErrors.NotFound(taskId));

            if (task.Status == PlanningTaskStatus.Active && string.IsNullOrWhiteSpace(request.CancellationReason))
                return Result.Failure(TaskApplicationErrors.CancellationReasonRequired);

            Result result = task.Cancel(request.CancelledAt, request.CancellationReason);

            if (result.IsFailure)
                return result;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
