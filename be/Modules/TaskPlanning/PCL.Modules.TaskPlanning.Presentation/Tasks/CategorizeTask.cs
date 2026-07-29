using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.TaskPlanning.Application.Tasks.CategorizeTask;
using PCL.Modules.TaskPlanning.Domain.Tasks;

namespace PCL.Modules.TaskPlanning.Presentation.Tasks
{
    internal sealed class CategorizeTask : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("tasks/{taskId:guid}/category", async (Guid taskId, CategorizeTaskRequest request, ISender sender) =>
            {
                var command = new CategorizeTaskCommand(
                    taskId,
                    request.OwnerId,
                    request.Category,
                    request.UpdatedAt ?? DateTimeOffset.UtcNow);

                Result result = await sender.Send(command);

                return result.Match(Results.NoContent, ApiResults.Problem);
            })
            .WithTags(Tags.Tasks);
        }

        internal sealed record CategorizeTaskRequest(
            Guid OwnerId,
            TaskCategory Category,
            DateTimeOffset? UpdatedAt);
    }
}

