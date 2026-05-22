using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using PCL.Modules.Session.Domain.LSessions;
using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Session.Application.Repositories;
using PCL.Modules.Session.Application.Abstractions.Data;

namespace PCL.Modules.Session.Application.LSessions.StartLearningSession
{
    public class StartLearningSessionCommandHandler(
        ILSessionRepository repository,
        IUnitOfWork unitOfWork) : ICommandHandler<StartLSessionCommand, Guid>
    {

        public async Task<Result<Guid>> Handle(StartLSessionCommand request, CancellationToken cancellationToken)
        {
            Result<LSession> startResult = LSession.StartNew(request.OwnerId, request.Title, request.StartedAt, request.ActivityIds);

            if (startResult.IsFailure)
                return Result.Failure<Guid>(startResult.Error);

            if (await repository.HasActiveSessionAsync(request.OwnerId, cancellationToken))
                return Result.Failure<Guid>(LSessionErrors.ActiveSessionAlreadyExists);

            LSession lSession = startResult.Value;

            repository.AddAsync(lSession);
            
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(lSession.Id);
        }
    }
}

