using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

namespace PCL.Modules.Evidence.Presentation.EvidenceItems;

internal sealed class UploadFileContent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "lsessions/{sessionId:guid}/evidence-items/{itemId:guid}/file-upload/content",
                async (
                    Guid sessionId,
                    Guid itemId,
                    Guid ownerId,
                    Guid uploadAttemptId,
                    HttpRequest request,
                    ISender sender
                ) =>
                {
                    var command = new UploadFileContentCommand(
                        sessionId,
                        itemId,
                        ownerId,
                        uploadAttemptId,
                        request.Body
                    );

                    Result result = await sender.Send(command);

                    return result.Match(Results.NoContent, ApiResults.Problem);
                }
            )
            .DisableAntiforgery()
            .WithTags(Tags.EvidenceItems);
    }
}
