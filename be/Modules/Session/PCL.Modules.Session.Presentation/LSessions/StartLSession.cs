using Common.Presentation.Results;
using Common.Presentation.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Session.Application.LSessions.StartLearningSession;
using Common.Domain;
using PCL.Modules.Session.Presentation;

namespace PCL.Modules.Session.Presentation.LSessions;

internal sealed class StartLearningSession : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("lsessions", async (StartRequest request, ISender sender) =>
        {
            var command = new StartLSessionCommand(
                request.Id,
                request.OwnerId,
                request.Title,
                request.StartedAt);

            Result<Guid> result = await sender.Send(command);

            return result.Match(
                value => Results.Created($"/api/lsessions/{value}", value),
                ApiResults.Problem);
        })
        .WithTags(Tags.LSessions);
    }

    internal sealed record StartRequest(
        Guid? Id,
        Guid OwnerId,
        string Title,
        DateTimeOffset StartedAt);
}
