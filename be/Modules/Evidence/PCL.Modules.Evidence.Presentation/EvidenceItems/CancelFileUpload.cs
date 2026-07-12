using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

namespace PCL.Modules.Evidence.Presentation.EvidenceItems;
internal sealed class CancelFileUpload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) => app.MapDelete(
        "lsessions/{sessionId:guid}/evidence-items/{itemId:guid}/file-upload",
        async (Guid sessionId, Guid itemId, Guid ownerId, Guid uploadAttemptId, ISender sender) =>
        {
            Result<FileUploadReadModel> result = await sender.Send(new CancelFileUploadCommand(sessionId, itemId, ownerId, uploadAttemptId, DateTimeOffset.UtcNow));
            return result.Match(Results.Ok, ApiResults.Problem);
        }).WithTags(Tags.EvidenceItems);
}
