using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

namespace PCL.Modules.Evidence.Presentation.EvidenceItems;

internal sealed class DownloadEvidenceFile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "lsessions/{sessionId:guid}/evidence-items/{itemId:guid}/file",
                async (Guid sessionId, Guid itemId, Guid ownerId, ISender sender) =>
                {
                    Result<FileDownloadReadModel> result = await sender.Send(
                        new DownloadEvidenceFileQuery(sessionId, itemId, ownerId)
                    );
                    return result.Match(
                        model =>
                            model.Url is not null
                                ? Results.Redirect(model.Url)
                                : Results.File(model.Content!, model.ContentType, model.FileName),
                        ApiResults.Problem
                    );
                }
            )
            .WithTags(Tags.EvidenceItems);
    }
}
