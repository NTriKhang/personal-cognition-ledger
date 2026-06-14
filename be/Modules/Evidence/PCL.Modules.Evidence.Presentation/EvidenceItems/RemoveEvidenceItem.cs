using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Evidence.Application.EvidenceItems.RemoveEvidenceItem;

namespace PCL.Modules.Evidence.Presentation.EvidenceItems;

internal sealed class RemoveEvidenceItem : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("lsessions/{sessionId:guid}/evidence-items/{evidenceItemId:guid}", async (
            Guid sessionId,
            Guid evidenceItemId,
            RemoveEvidenceItemRequest request,
            ISender sender) =>
        {
            var command = new RemoveEvidenceItemCommand(
                sessionId,
                evidenceItemId,
                request.OwnerId,
                request.RemovedAt ?? DateTimeOffset.UtcNow,
                request.RemovalReason);

            Result result = await sender.Send(command);

            return result.Match(
                () => Results.NoContent(),
                ApiResults.Problem);
        })
        .WithTags(Tags.EvidenceItems);
    }

    internal sealed record RemoveEvidenceItemRequest(
        Guid OwnerId,
        DateTimeOffset? RemovedAt,
        string? RemovalReason);
}
