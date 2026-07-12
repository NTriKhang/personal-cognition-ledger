using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

namespace PCL.Modules.Evidence.Presentation.EvidenceItems;
internal sealed class RenewFileUpload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) => app.MapPost(
        "lsessions/{sessionId:guid}/evidence-items/{itemId:guid}/file-upload/renew",
        async (Guid sessionId, Guid itemId, Request request, ISender sender) =>
        {
            Result<FileUploadReadModel> result = await sender.Send(new RenewFileUploadCommand(sessionId, itemId, request.OwnerId, request.UploadAttemptId, DateTimeOffset.UtcNow));
            return result.Match(model =>
            {
                if (model.UploadMode == "ApiProxy") model = model with { UploadUrl = $"/lsessions/{sessionId}/evidence-items/{itemId}/file-upload/content?ownerId={request.OwnerId}&uploadAttemptId={model.UploadAttemptId}" };
                return Results.Ok(model);
            }, ApiResults.Problem);
        }).WithTags(Tags.EvidenceItems);
    internal sealed record Request(Guid OwnerId, Guid UploadAttemptId);
}
