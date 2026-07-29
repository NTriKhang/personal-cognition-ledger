using System.Data.Common;
using System.Text;
using Common.Application.Data;
using Common.Application.Messaging;
using Common.Domain;
using Dapper;

namespace PCL.Modules.TaskPlanning.Application.Tasks.ListTasks
{
    internal sealed class ListTasksQueryHandler(IDbConnectionFactory dbConnectionFactory)
        : IQueryHandler<ListTasksQuery, IReadOnlyCollection<TaskSummaryReadModel>>
    {
        public async Task<Result<IReadOnlyCollection<TaskSummaryReadModel>>> Handle(
            ListTasksQuery request,
            CancellationToken cancellationToken)
        {
            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

            var sql = new StringBuilder(
            $"""
             SELECT
                 "Id" AS {nameof(TaskSummaryReadModel.Id)},
                 "Code" AS {nameof(TaskSummaryReadModel.Code)},
                 "OwnerId" AS {nameof(TaskSummaryReadModel.OwnerId)},
                 "Title" AS {nameof(TaskSummaryReadModel.Title)},
                 "Status" AS {nameof(TaskSummaryReadModel.Status)},
                 "Category" AS {nameof(TaskSummaryReadModel.Category)},
                 "Priority" AS {nameof(TaskSummaryReadModel.Priority)},
                 "CreatedAt" AS {nameof(TaskSummaryReadModel.CreatedAt)},
                 "UpdatedAt" AS {nameof(TaskSummaryReadModel.UpdatedAt)},
                 "CompletedAt" AS {nameof(TaskSummaryReadModel.CompletedAt)},
                 "CancelledAt" AS {nameof(TaskSummaryReadModel.CancelledAt)}
             FROM task_planning.task
             WHERE "OwnerId" = @OwnerId 
             """);

            var parameters = new DynamicParameters();
            parameters.Add("OwnerId", request.OwnerId);

            if (request.AssignableOnly)
            {
                sql.AppendLine("""AND "Status" IN ('Planned', 'Active')""");
            }
            else if (request.Status.HasValue)
            {
                sql.AppendLine("""AND "Status" = @Status""");
                parameters.Add("Status", request.Status.Value.ToString());
            }

            if (request.Category.HasValue)
            {
                sql.AppendLine("""AND "Category" = @Category""");
                parameters.Add("Category", request.Category.Value.ToString());
            }

            if (request.Priority.HasValue)
            {
                sql.AppendLine("""AND "Priority" = @Priority""");
                parameters.Add("Priority", request.Priority.Value.ToString());
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                sql.AppendLine("""AND "Title" ILIKE @Search""");
                parameters.Add("Search", $"%{request.Search.Trim()}%");
            }

            if (request.CreatedFrom.HasValue)
            {
                sql.AppendLine("""AND "CreatedAt" >= @CreatedFrom""");
                parameters.Add("CreatedFrom", request.CreatedFrom.Value);
            }

            if (request.CreatedTo.HasValue)
            {
                sql.AppendLine("""AND "CreatedAt" <= @CreatedTo""");
                parameters.Add("CreatedTo", request.CreatedTo.Value);
            }

            sql.AppendLine("""ORDER BY "CreatedAt" DESC""");

            List<TaskSummaryReadModel> tasks =
                (await connection.QueryAsync<TaskSummaryReadModel>(sql.ToString(), parameters)).AsList();

            return Result.Success<IReadOnlyCollection<TaskSummaryReadModel>>(tasks);
        }
    }
}
