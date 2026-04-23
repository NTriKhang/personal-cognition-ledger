using System.Threading;
using System.Threading.Tasks;
using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Session.Domain.LSessions;
using PCL.Modules.Session.Application.Repositories;

namespace PCL.Modules.Session.Application.LSessions.GetLearningSession
{
    public class GetLearningSessionQueryHandler(
        AutoMapper.IMapper mapper,
        ILSessionRepository repository)
        : IQueryHandler<GetLearningSessionQuery, LSessionDto>
    {
        public async Task<Result<LSessionDto>> Handle(GetLearningSessionQuery request, CancellationToken cancellationToken)
        {
            var session = await repository.GetByIdAsync(request.Id);

            if (session is null)
                return Result.Failure<LSessionDto>(LSessionErrors.NotFound(request.Id));

            var dto = mapper.Map<LSessionDto>(session);

            return Result.Success(dto);
        }
    }
}