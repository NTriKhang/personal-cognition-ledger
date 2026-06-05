using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Session.Application.LSessions.AssignTaskToSession;

namespace PCL.Modules.Session.Presentation.LSessions;

internal sealed class AssignTaskToSession : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("lsessions/{id:guid}/tasks/{taskId:guid}", async (
            Guid id,
            Guid taskId,
            AssignTaskRequest request,
            ISender sender) =>
        {
            var command = new AssignTaskToSessionCommand(
                id,
                taskId,
                request.AssignedAt ?? DateTimeOffset.UtcNow);

            Result result = await sender.Send(command);

            return result.Match(
                () => Results.Ok(),
                ApiResults.Problem);
        })
        .WithTags(Tags.LSessions);
    }

    internal sealed record AssignTaskRequest(DateTimeOffset? AssignedAt);
}
