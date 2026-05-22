using System.Data.Common;
using Common.Application.Data;
using Common.Application.Messaging;
using Common.Domain;
using Dapper;
using PCL.Modules.TaskPlanning.Domain.Tasks;

namespace PCL.Modules.TaskPlanning.Application.Tasks.GetTaskById
{
    internal sealed class GetTaskByIdQueryHandler(IDbConnectionFactory dbConnectionFactory)
        : IQueryHandler<GetTaskByIdQuery, TaskDetailReadModel>
    {
        public async Task<Result<TaskDetailReadModel>> Handle(
            GetTaskByIdQuery request,
            CancellationToken cancellationToken)
        {
            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

            const string sql =
            $"""
             SELECT
                 "Id" AS {nameof(TaskDetailReadModel.Id)},
                 "Code" AS {nameof(TaskDetailReadModel.Code)},
                 "OwnerId" AS {nameof(TaskDetailReadModel.OwnerId)},
                 "Title" AS {nameof(TaskDetailReadModel.Title)},
                 "Description" AS {nameof(TaskDetailReadModel.Description)},
                 "Status" AS {nameof(TaskDetailReadModel.Status)},
                 "Category" AS {nameof(TaskDetailReadModel.Category)},
                 "Priority" AS {nameof(TaskDetailReadModel.Priority)},
                 "CreatedAt" AS {nameof(TaskDetailReadModel.CreatedAt)},
                 "UpdatedAt" AS {nameof(TaskDetailReadModel.UpdatedAt)},
                 "PlannedAt" AS {nameof(TaskDetailReadModel.PlannedAt)},
                 "ActivatedAt" AS {nameof(TaskDetailReadModel.ActivatedAt)},
                 "CompletedAt" AS {nameof(TaskDetailReadModel.CompletedAt)},
                 "CancelledAt" AS {nameof(TaskDetailReadModel.CancelledAt)},
                 "CompletionNote" AS {nameof(TaskDetailReadModel.CompletionNote)},
                 "CancellationReason" AS {nameof(TaskDetailReadModel.CancellationReason)}
             FROM task_planning.task
             WHERE "Id" = @TaskId AND "OwnerId" = @OwnerId
             """;

            TaskDetailReadModel? task =
                await connection.QuerySingleOrDefaultAsync<TaskDetailReadModel>(
                    sql,
                    new { request.TaskId, request.OwnerId });

            if (task is null)
                return Result.Failure<TaskDetailReadModel>(TaskErrors.NotFound(TaskId.From(request.TaskId)));

            return Result.Success(task);
        }
    }
}
