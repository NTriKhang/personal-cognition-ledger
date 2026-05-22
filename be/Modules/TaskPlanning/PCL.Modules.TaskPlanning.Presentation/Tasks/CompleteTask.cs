using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.TaskPlanning.Application.Tasks.CompleteTask;

namespace PCL.Modules.TaskPlanning.Presentation.Tasks
{
    internal sealed class CompleteTask : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("tasks/{taskId:guid}/complete", async (Guid taskId, CompleteTaskRequest request, ISender sender) =>
            {
                var command = new CompleteTaskCommand(
                    taskId,
                    request.OwnerId,
                    request.CompletedAt ?? DateTimeOffset.UtcNow,
                    request.CompletionNote);

                Result result = await sender.Send(command);

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .WithTags(Tags.Tasks);
        }

        internal sealed record CompleteTaskRequest(
            Guid OwnerId,
            DateTimeOffset? CompletedAt,
            string? CompletionNote);
    }
}

