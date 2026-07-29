using System.Threading;
using System.Threading.Tasks;
using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Session.Domain.LSessions;
using PCL.Modules.Session.Application.Repositories;
using PCL.Modules.Session.Application.Abstractions.Data;

namespace PCL.Modules.Session.Application.LSessions.RemoveTaskFromSession
{
    public class RemoveTaskFromSessionCommandHandler(
        ILSessionRepository repository,
        IUnitOfWork unitOfWork)
        : ICommandHandler<RemoveTaskFromSessionCommand>
    {
        public async Task<Result> Handle(RemoveTaskFromSessionCommand request, CancellationToken cancellationToken)
        {
            var session = await repository.GetAsync(request.SessionId);

            if (session is null)
                return Result.Failure<Guid>(LSessionErrors.NotFound(request.SessionId));

            Result removeResult = session.RemoveAssignedTask(request.TaskId);

            if (removeResult.IsFailure)
            {
                return removeResult;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(session.Id);
        }
    }
}
