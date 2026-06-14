using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Evidence.Application.EvidenceItems.AddEvidenceItem;
using PCL.Modules.Evidence.Domain.EvidenceItems;

namespace PCL.Modules.Evidence.Presentation.EvidenceItems;

internal sealed class AddEvidenceItem : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("lsessions/{sessionId:guid}/evidence-items", async (
            Guid sessionId,
            AddEvidenceItemRequest request,
            ISender sender) =>
        {
            var command = new AddEvidenceItemCommand(
                sessionId,
                request.OwnerId,
                request.Type,
                request.Content,
                request.AddedAt ?? DateTimeOffset.UtcNow);

            Result<Guid> result = await sender.Send(command);

            return result.Match(
                id => Results.Created($"/api/lsessions/{sessionId}/evidence-items/{id}", id),
                ApiResults.Problem);
        })
        .WithTags(Tags.EvidenceItems);
    }

    internal sealed record AddEvidenceItemRequest(
        Guid OwnerId,
        EvidenceItemType Type,
        string Content,
        DateTimeOffset? AddedAt);
}
