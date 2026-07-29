using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.TaskPlanning.Application.Tasks.PrioritizeTask;
using PCL.Modules.TaskPlanning.Domain.Tasks;

namespace PCL.Modules.TaskPlanning.Presentation.Tasks
{
    internal sealed class PrioritizeTask : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("tasks/{taskId:guid}/priority", async (Guid taskId, PrioritizeTaskRequest request, ISender sender) =>
            {
                var command = new PrioritizeTaskCommand(
                    taskId,
                    request.OwnerId,
                    request.Priority,
                    request.UpdatedAt ?? DateTimeOffset.UtcNow);

                Result result = await sender.Send(command);

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .WithTags(Tags.Tasks);
        }

        internal sealed record PrioritizeTaskRequest(
            Guid OwnerId,
            TaskPriority Priority,
            DateTimeOffset? UpdatedAt);
    }
}

