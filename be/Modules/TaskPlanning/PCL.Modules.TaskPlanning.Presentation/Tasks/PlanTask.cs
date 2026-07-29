using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.TaskPlanning.Application.Tasks.PlanTask;

namespace PCL.Modules.TaskPlanning.Presentation.Tasks
{
    internal sealed class PlanTask : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("tasks/{taskId:guid}/plan", async (Guid taskId, PlanTaskRequest request, ISender sender) =>
            {
                var command = new PlanTaskCommand(
                    taskId,
                    request.OwnerId,
                    request.PlannedAt ?? DateTimeOffset.UtcNow);

                Result result = await sender.Send(command);

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .WithTags(Tags.Tasks);
        }

        internal sealed record PlanTaskRequest(Guid OwnerId, DateTimeOffset? PlannedAt);
    }
}

