using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Session.Application.LSessions.RemoveTaskFromSession;

namespace PCL.Modules.Session.Presentation.LSessions;

internal sealed class RemoveTaskFromSession : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("lsessions/{id:guid}/tasks/{taskId:guid}", async (
            Guid id,
            Guid taskId,
            ISender sender) =>
        {
            Result result = await sender.Send(new RemoveTaskFromSessionCommand(id, taskId));

            return result.Match(
                () => Results.NoContent(),
                ApiResults.Problem);
        })
        .WithTags(Tags.LSessions);
    }
}
