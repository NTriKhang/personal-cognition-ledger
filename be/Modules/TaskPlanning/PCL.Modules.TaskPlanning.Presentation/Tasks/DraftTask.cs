using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.TaskPlanning.Application.Tasks.DraftTask;

namespace PCL.Modules.TaskPlanning.Presentation.Tasks
{
    internal sealed class DraftTask : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("tasks/draft", async (DraftTaskRequest request, ISender sender) =>
            {
                var command = new DraftTaskCommand(
                    request.OwnerId,
                    request.Title,
                    request.Description,
                    request.CreatedAt ?? DateTimeOffset.UtcNow);

                Result<Guid> result = await sender.Send(command);

                return result.Match(
                    value => Results.Created($"/api/tasks/{value}", value),
                    ApiResults.Problem);
            })
            .WithTags(Tags.Tasks);
        }

        internal sealed record DraftTaskRequest(
            Guid OwnerId,
            string Title,
            string? Description,
            DateTimeOffset? CreatedAt);
    }
}

