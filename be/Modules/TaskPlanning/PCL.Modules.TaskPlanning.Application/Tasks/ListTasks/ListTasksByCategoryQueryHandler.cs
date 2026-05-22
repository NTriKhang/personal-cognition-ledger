using Common.Application.Data;
using Common.Application.Messaging;
using Common.Domain;

namespace PCL.Modules.TaskPlanning.Application.Tasks.ListTasks
{
    internal sealed class ListTasksByCategoryQueryHandler(IDbConnectionFactory dbConnectionFactory)
        : IQueryHandler<ListTasksByCategoryQuery, IReadOnlyCollection<TaskSummaryReadModel>>
    {
        public Task<Result<IReadOnlyCollection<TaskSummaryReadModel>>> Handle(
            ListTasksByCategoryQuery request,
            CancellationToken cancellationToken)
        {
            var handler = new ListTasksQueryHandler(dbConnectionFactory);

            return handler.Handle(
                new ListTasksQuery(request.OwnerId, null, request.Category, null, null, null, null),
                cancellationToken);
        }
    }
}
