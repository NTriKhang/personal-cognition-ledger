using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.TaskPlanning.Application.Tasks.GetTaskById;

namespace PCL.Modules.TaskPlanning.Presentation.Tasks
{
    internal sealed class GetTask : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("tasks/{taskId:guid}", async (Guid taskId, Guid ownerId, ISender sender) =>
            {
                Result<TaskDetailReadModel> result =
                    await sender.Send(new GetTaskByIdQuery(taskId, ownerId));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Tasks);
        }
    }
}

