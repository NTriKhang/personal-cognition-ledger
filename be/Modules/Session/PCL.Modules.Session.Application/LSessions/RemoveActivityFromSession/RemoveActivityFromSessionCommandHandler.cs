using System.Threading;
using System.Threading.Tasks;
using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Session.Domain.LSessions;
using PCL.Modules.Session.Application.Repositories;
using PCL.Modules.Session.Application.Abstractions.Data;

namespace PCL.Modules.Session.Application.LSessions.RemoveActivityFromSession
{
    public class RemoveActivityFromSessionCommandHandler(
        ILSessionRepository repository,
        IUnitOfWork unitOfWork)
        : ICommandHandler<RemoveActivityFromSessionCommand>
    {
        public async Task<Result> Handle(RemoveActivityFromSessionCommand request, CancellationToken cancellationToken)
        {
            var session = await repository.GetAsync(request.SessionId);

            if (session is null)
                return Result.Failure<Guid>(LSessionErrors.NotFound(request.SessionId));

            session.RemoveActivity(request.ActivityId);

            await unitOfWork.SaveChangesAsync();

            return Result.Success(session.Id);
        }
    }
}