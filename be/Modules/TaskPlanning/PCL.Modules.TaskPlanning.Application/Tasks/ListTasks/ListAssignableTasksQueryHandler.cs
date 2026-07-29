using Common.Application.Data;
using Common.Application.Messaging;
using Common.Domain;

namespace PCL.Modules.TaskPlanning.Application.Tasks.ListTasks
{
    internal sealed class ListAssignableTasksQueryHandler(IDbConnectionFactory dbConnectionFactory)
        : IQueryHandler<ListAssignableTasksQuery, IReadOnlyCollection<TaskSummaryReadModel>>
    {
        public Task<Result<IReadOnlyCollection<TaskSummaryReadModel>>> Handle(
            ListAssignableTasksQuery request,
            CancellationToken cancellationToken)
        {
            var handler = new ListTasksQueryHandler(dbConnectionFactory);

            return handler.Handle(
                new ListTasksQuery(request.OwnerId, null, null, null, null, null, null, AssignableOnly: true),
                cancellationToken);
        }
    }
}
