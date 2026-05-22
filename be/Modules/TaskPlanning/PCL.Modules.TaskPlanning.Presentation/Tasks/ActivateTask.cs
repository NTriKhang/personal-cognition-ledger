using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.TaskPlanning.Application.Tasks.ActivateTask;

namespace PCL.Modules.TaskPlanning.Presentation.Tasks
{
    internal sealed class ActivateTask : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("tasks/{taskId:guid}/activate", async (Guid taskId, ActivateTaskRequest request, ISender sender) =>
            {
                var command = new ActivateTaskCommand(
                    taskId,
                    request.OwnerId,
                    request.ActivatedAt ?? DateTimeOffset.UtcNow);

                Result result = await sender.Send(command);

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .WithTags(Tags.Tasks);
        }

        internal sealed record ActivateTaskRequest(Guid OwnerId, DateTimeOffset? ActivatedAt);
    }
}

