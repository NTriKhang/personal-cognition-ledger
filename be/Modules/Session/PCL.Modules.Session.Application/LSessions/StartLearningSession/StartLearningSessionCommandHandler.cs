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
            LSession lSession = LSession.StartNew(request.StartedAt, request.ActivityIds);

            repository.AddAsync(lSession);
            
            await unitOfWork.SaveChangesAsync();

            return Result.Success(lSession.Id);
        }
    }
}

