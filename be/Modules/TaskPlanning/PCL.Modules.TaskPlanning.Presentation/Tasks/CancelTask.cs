using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.TaskPlanning.Application.Tasks.CancelTask;

namespace PCL.Modules.TaskPlanning.Presentation.Tasks
{
    internal sealed class CancelTask : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("tasks/{taskId:guid}/cancel", async (Guid taskId, CancelTaskRequest request, ISender sender) =>
            {
                var command = new CancelTaskCommand(
                    taskId,
                    request.OwnerId,
                    request.CancelledAt ?? DateTimeOffset.UtcNow,
                    request.CancellationReason);

                Result result = await sender.Send(command);

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .WithTags(Tags.Tasks);
        }

        internal sealed record CancelTaskRequest(
            Guid OwnerId,
            DateTimeOffset? CancelledAt,
            string? CancellationReason);
    }
}

