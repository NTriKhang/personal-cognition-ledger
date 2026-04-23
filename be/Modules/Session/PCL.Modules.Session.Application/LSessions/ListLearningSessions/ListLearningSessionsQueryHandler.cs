using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Session.Application.LSessions.GetLearningSession;
using PCL.Modules.Session.Application.Repositories;

namespace PCL.Modules.Session.Application.LSessions.ListLearningSessions
{
    public class ListLearningSessionsQueryHandler(
        AutoMapper.IMapper mapper,
        ILSessionRepository repository)
        : IQueryHandler<ListLearningSessionsQuery, IEnumerable<LSessionDto>>
    {
        public async Task<Result<IEnumerable<LSessionDto>>> Handle(ListLearningSessionsQuery request, CancellationToken cancellationToken)
        {
            var sessions = await repository.ListAsync();

            var dtos = sessions.Select(mapper.Map<LSessionDto>);

            return Result.Success(dtos);
        }
    }
}