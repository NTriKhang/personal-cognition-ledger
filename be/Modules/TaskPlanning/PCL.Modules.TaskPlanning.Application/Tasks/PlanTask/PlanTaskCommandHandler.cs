using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.TaskPlanning.Application.Abstractions.Data;
using PCL.Modules.TaskPlanning.Application.Repositories;
using PCL.Modules.TaskPlanning.Domain.Tasks;

namespace PCL.Modules.TaskPlanning.Application.Tasks.PlanTask
{
    internal sealed class PlanTaskCommandHandler(
        ITaskRepository repository,
        IUnitOfWork unitOfWork) : ICommandHandler<PlanTaskCommand>
    {
        public async Task<Result> Handle(PlanTaskCommand request, CancellationToken cancellationToken)
        {
            TaskId taskId = TaskId.From(request.TaskId);
            Domain.Tasks.Task? task = await repository.GetAsync(taskId, cancellationToken);

            if (task is null || task.OwnerId != request.OwnerId)
                return Result.Failure(TaskErrors.NotFound(taskId));

            Result result = task.Plan(request.PlannedAt);

            if (result.IsFailure)
                return result;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

