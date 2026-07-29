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
                 "Id" AS {nameof(LSessionDto.Id)},
                 "OwnerId" AS {nameof(LSessionDto.OwnerId)},
                 "Code" AS {nameof(LSessionDto.Code)},
                 "Title" AS {nameof(LSessionDto.Title)},
                 "StartedAt" AS {nameof(LSessionDto.StartedAt)},
                 "EndedAt" AS {nameof(LSessionDto.EndedAt)},
                 "Status" AS {nameof(LSessionDto.Status)}
             FROM session.lsession
             """;

            List<LSessionDto> sessions =
                (await connection.QueryAsync<LSessionDto>(sql)).AsList();

            if (sessions.Count == 0)
            {
                return Result.Success<IReadOnlyCollection<LSessionDto>>(sessions);
            }

            const string assignmentSql =
            """
            SELECT
                "SessionId",
                "TaskId"
            FROM session.session_task_assignments
            WHERE "SessionId" = ANY(@SessionIds)
            """;

            IEnumerable<SessionTaskAssignmentRow> assignments =
                await connection.QueryAsync<SessionTaskAssignmentRow>(
                    assignmentSql,
                    new { SessionIds = sessions.Select(session => session.Id).ToArray() });

            Dictionary<Guid, List<Guid>> assignedTaskIdsBySessionId = assignments
                .GroupBy(assignment => assignment.SessionId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(assignment => assignment.TaskId).ToList());

            foreach (LSessionDto session in sessions)
            {
                if (assignedTaskIdsBySessionId.TryGetValue(session.Id, out List<Guid>? assignedTaskIds))
                {
                    session.AssignedTaskIds = assignedTaskIds;
                }
            }

            return Result.Success<IReadOnlyCollection<LSessionDto>>(sessions);
        }

        private sealed class SessionTaskAssignmentRow
        {
            public Guid SessionId { get; init; }
            public Guid TaskId { get; init; }
        }
    }
}
