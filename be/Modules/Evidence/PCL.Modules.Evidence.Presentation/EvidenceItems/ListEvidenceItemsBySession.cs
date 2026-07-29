using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Evidence.Application.EvidenceItems.ListEvidenceItemsBySession;

namespace PCL.Modules.Evidence.Presentation.EvidenceItems;

internal sealed class ListEvidenceItemsBySession : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("lsessions/{sessionId:guid}/evidence-items", async (
            Guid sessionId,
            Guid ownerId,
            ISender sender,
            bool includeRemoved = false) =>
        {
            var query = new ListEvidenceItemsBySessionQuery(sessionId, ownerId, includeRemoved);

            Result<IReadOnlyCollection<EvidenceItemReadModel>> result = await sender.Send(query);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.EvidenceItems);
    }
}
