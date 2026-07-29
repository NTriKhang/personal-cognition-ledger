using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.TaskPlanning.Application.Abstractions.Data;
using PCL.Modules.TaskPlanning.Application.Repositories;
using PCL.Modules.TaskPlanning.Domain.Tasks;
using PlanningTask = PCL.Modules.TaskPlanning.Domain.Tasks.Task;
using PlanningTaskStatus = PCL.Modules.TaskPlanning.Domain.Tasks.TaskStatus;

namespace PCL.Modules.TaskPlanning.Application.Tasks.RecordTaskAssignedToSession;

internal sealed class RecordTaskAssignedToSessionCommandHandler(
    ITaskRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RecordTaskAssignedToSessionCommand>
{
    public async Task<Result> Handle(
        RecordTaskAssignedToSessionCommand request,
        CancellationToken cancellationToken)
    {
        TaskId taskId = TaskId.From(request.TaskId);
        PlanningTask? task = await repository.GetAsync(taskId, cancellationToken);

        if (task is null || task.OwnerId != request.OwnerId)
        {
            return Result.Failure(TaskErrors.NotFound(taskId));
        }

        if (task.Status == PlanningTaskStatus.Active)
        {
            return Result.Success();
        }

        if (task.Status != PlanningTaskStatus.Planned)
        {
            return Result.Failure(TaskErrors.CannotActivate);
        }

        Result result = task.Activate(request.AssignedAt);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
