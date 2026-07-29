using System.Threading;
using System.Threading.Tasks;
using PCL.Modules.Session.Domain.LSessions;
using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Session.Application.Repositories;
using PCL.Modules.Session.Application.Abstractions.Data;

namespace PCL.Modules.Session.Application.LSessions.AssignTaskToSession
{
    sealed class AssignTaskToSessionCommandHandler(
        ILSessionRepository repository,
        IAssignTaskToSessionPolicy assignmentPolicy,
        IUnitOfWork unitOfWork) : ICommandHandler<AssignTaskToSessionCommand>
    {
        public async Task<Result> Handle(AssignTaskToSessionCommand request, CancellationToken cancellationToken)
        {
            LSession? session = await repository.GetAsync(request.SessionId);

            if (session == null)
            {
                return Result.Failure(LSessionErrors.NotFound(request.SessionId));
            }

            Result validationResult = await assignmentPolicy.ValidateAsync(
                session,
                request.TaskId,
                cancellationToken);

            if (validationResult.IsFailure)
            {
                return validationResult;
            }

            Result assignResult = session.AssignTask(request.TaskId, request.AssignedAt);

            if (assignResult.IsFailure)
            {
                return assignResult;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

