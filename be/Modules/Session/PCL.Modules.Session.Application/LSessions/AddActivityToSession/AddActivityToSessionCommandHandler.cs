using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PCL.Modules.Session.Domain.LSessions;
using Common.Application.Messaging;
using Common.Domain;
using System.Data.Common;
using PCL.Modules.Session.Application.Repositories;
using PCL.Modules.Session.Application.Abstractions.Data;

namespace PCL.Modules.Session.Application.LSessions.AddActivityToSession
{
    sealed class AddActivityToSessionCommandHandler(
        ILSessionRepository repository,
        IUnitOfWork unitOfWork) : ICommandHandler<AddActivityToSessionCommand>
    {
        public async Task<Result> Handle(AddActivityToSessionCommand request, CancellationToken cancellationToken)
        {
            LSession? session = await repository.GetAsync(request.SessionId);

            if (session == null)
            {
                return Result.Failure(LSessionErrors.NotFound(request.SessionId));
            }

            session.AddActivity(request.ActivityId);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}

