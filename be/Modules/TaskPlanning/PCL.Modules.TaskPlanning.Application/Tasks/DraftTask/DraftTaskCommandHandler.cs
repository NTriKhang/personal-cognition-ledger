using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.TaskPlanning.Application.Abstractions.Data;
using PCL.Modules.TaskPlanning.Application.Repositories;
using PlanningTask = PCL.Modules.TaskPlanning.Domain.Tasks.Task;

namespace PCL.Modules.TaskPlanning.Application.Tasks.DraftTask
{
    internal sealed class DraftTaskCommandHandler(
        ITaskRepository repository,
        IUnitOfWork unitOfWork) : ICommandHandler<DraftTaskCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(DraftTaskCommand request, CancellationToken cancellationToken)
        {
            Result<PlanningTask> draftResult = PlanningTask.Draft(
                request.OwnerId,
                request.Title,
                request.Description,
                request.CreatedAt);

            if (draftResult.IsFailure)
                return Result.Failure<Guid>(draftResult.Error);

            PlanningTask task = draftResult.Value;

            repository.Add(task);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(task.Id.Value);
        }
    }
}

