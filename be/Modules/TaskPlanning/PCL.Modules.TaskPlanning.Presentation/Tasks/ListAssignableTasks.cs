using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.TaskPlanning.Application.Tasks.ListTasks;

namespace PCL.Modules.TaskPlanning.Presentation.Tasks
{
    internal sealed class ListAssignableTasks : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("tasks/assignable", async (Guid ownerId, ISender sender) =>
            {
                Result<IReadOnlyCollection<TaskSummaryReadModel>> result =
                    await sender.Send(new ListAssignableTasksQuery(ownerId));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Tasks);
        }
    }
}

