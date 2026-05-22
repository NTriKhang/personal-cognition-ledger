using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.TaskPlanning.Application.Tasks.ListTasks;
using PCL.Modules.TaskPlanning.Domain.Tasks;
using PlanningTaskStatus = PCL.Modules.TaskPlanning.Domain.Tasks.TaskStatus;

namespace PCL.Modules.TaskPlanning.Presentation.Tasks
{
    internal sealed class ListTasks : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("tasks", async (
                Guid ownerId,
                PlanningTaskStatus? status,
                TaskCategory? category,
                TaskPriority? priority,
                string? search,
                DateTimeOffset? createdFrom,
                DateTimeOffset? createdTo,
                ISender sender) =>
            {
                var query = new ListTasksQuery(
                    ownerId,
                    status,
                    category,
                    priority,
                    search,
                    createdFrom,
                    createdTo);

                Result<IReadOnlyCollection<TaskSummaryReadModel>> result = await sender.Send(query);

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Tasks);
        }
    }
}
