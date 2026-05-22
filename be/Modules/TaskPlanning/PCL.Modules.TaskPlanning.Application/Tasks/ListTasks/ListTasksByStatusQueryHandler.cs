using Common.Application.Data;
using Common.Application.Messaging;
using Common.Domain;

namespace PCL.Modules.TaskPlanning.Application.Tasks.ListTasks
{
    internal sealed class ListTasksByStatusQueryHandler(IDbConnectionFactory dbConnectionFactory)
        : IQueryHandler<ListTasksByStatusQuery, IReadOnlyCollection<TaskSummaryReadModel>>
    {
        public Task<Result<IReadOnlyCollection<TaskSummaryReadModel>>> Handle(
            ListTasksByStatusQuery request,
            CancellationToken cancellationToken)
        {
            var handler = new ListTasksQueryHandler(dbConnectionFactory);

            return handler.Handle(
                new ListTasksQuery(request.OwnerId, request.Status, null, null, null, null, null),
                cancellationToken);
        }
    }
}
