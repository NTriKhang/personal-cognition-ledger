using System.Threading;
using System.Threading.Tasks;
using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Session.Domain.LSessions;
using PCL.Modules.Session.Application.Repositories;
using PCL.Modules.Session.Application.Abstractions.Data;

namespace PCL.Modules.Session.Application.LSessions.EndLearningSession
{
    public class EndLearningSessionCommandHandler(
        ILSessionRepository repository,
        IUnitOfWork unitOfWork)
        : ICommandHandler<EndLearningSessionCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(EndLearningSessionCommand request, CancellationToken cancellationToken)
        {
            var session = await repository.GetByIdAsync(request.Id);

            if (session is null)
                return Result.Failure<Guid>(LSessionErrors.NotFound(request.Id));

            var result = session.End(request.EndedAt);

            if (result.IsFailure)
                return Result.Failure<Guid>(result.Error);

            await unitOfWork.SaveChangesAsync();

            return Result.Success(session.Id);
        }
    }
}