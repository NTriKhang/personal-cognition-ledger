using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.TaskPlanning.Application.Tasks.RefineTask;

namespace PCL.Modules.TaskPlanning.Presentation.Tasks
{
    internal sealed class RefineTask : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("tasks/{taskId:guid}/details", async (Guid taskId, RefineTaskRequest request, ISender sender) =>
            {
                var command = new RefineTaskCommand(
                    taskId,
                    request.OwnerId,
                    request.Title,
                    request.Description,
                    request.UpdatedAt ?? DateTimeOffset.UtcNow);

                Result result = await sender.Send(command);

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .WithTags(Tags.Tasks);
        }

        internal sealed record RefineTaskRequest(
            Guid OwnerId,
            string Title,
            string? Description,
            DateTimeOffset? UpdatedAt);
    }
}

