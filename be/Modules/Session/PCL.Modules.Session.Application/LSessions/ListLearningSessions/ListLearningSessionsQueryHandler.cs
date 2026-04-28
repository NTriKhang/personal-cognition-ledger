using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Application.Data;
using Common.Application.Messaging;
using Common.Domain;
using Dapper;
using PCL.Modules.Session.Application.LSessions.GetLearningSession;

namespace PCL.Modules.Session.Application.LSessions.ListLearningSessions
{
    internal sealed class ListLearningSessionsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<ListLearningSessionsQuery, IReadOnlyCollection<LSessionDto>>
    {
        public async Task<Result<IReadOnlyCollection<LSessionDto>>> Handle(
            ListLearningSessionsQuery request,
            CancellationToken cancellationToken)
        {
            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

            const string sql =
                $"""
             SELECT
                 id AS {nameof(LSessionDto.Id)},
                 code AS {nameof(LSessionDto.Code)},
                 started_at AS {nameof(LSessionDto.StartedAt)},
                 ended_at AS {nameof(LSessionDto.EndedAt)},
                 status AS {nameof(LSessionDto.Status)}
             FROM session.learning_sessions
             """;

            List<LSessionDto> sessions =
                (await connection.QueryAsync<LSessionDto>(sql)).AsList();

            return Result.Success<IReadOnlyCollection<LSessionDto>>(sessions);
        }
    }
}