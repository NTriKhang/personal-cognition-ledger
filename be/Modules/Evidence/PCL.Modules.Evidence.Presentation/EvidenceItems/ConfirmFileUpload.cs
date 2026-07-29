using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

namespace PCL.Modules.Evidence.Presentation.EvidenceItems;

internal sealed class ConfirmFileUpload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "lsessions/{sessionId:guid}/evidence-items/{itemId:guid}/file-upload/confirm",
                async (Guid sessionId, Guid itemId, Request request, ISender sender) =>
                {
                    var command = new ConfirmFileUploadCommand(
                        sessionId,
                        itemId,
                        request.OwnerId,
                        request.UploadAttemptId,
                        DateTimeOffset.UtcNow
                    );

                    Result<FileUploadReadModel> result = await sender.Send(command);

                    return result.Match(Results.Ok, ApiResults.Problem);
                }
            )
            .WithTags(Tags.EvidenceItems);
    }

    internal sealed record Request(Guid OwnerId, Guid UploadAttemptId);
}
