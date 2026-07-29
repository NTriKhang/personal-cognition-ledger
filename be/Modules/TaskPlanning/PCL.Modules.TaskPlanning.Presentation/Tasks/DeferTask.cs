using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.TaskPlanning.Application.Tasks.DeferTask;

namespace PCL.Modules.TaskPlanning.Presentation.Tasks
{
    internal sealed class DeferTask : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("tasks/{taskId:guid}/defer", async (Guid taskId, DeferTaskRequest request, ISender sender) =>
            {
                var command = new DeferTaskCommand(
                    taskId,
                    request.OwnerId,
                    request.DeferredAt ?? DateTimeOffset.UtcNow);

                Result result = await sender.Send(command);

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .WithTags(Tags.Tasks);
        }

        internal sealed record DeferTaskRequest(Guid OwnerId, DateTimeOffset? DeferredAt);
    }
}

