using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

namespace PCL.Modules.Evidence.Presentation.EvidenceItems;

internal sealed class InitializeFileUpload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "lsessions/{sessionId:guid}/evidence-items/file-uploads",
                async (Guid sessionId, Request request, ISender sender) =>
                {
                    Result<FileUploadReadModel> result = await sender.Send(
                        new InitializeFileUploadCommand(
                            sessionId,
                            request.OwnerId,
                            request.Caption,
                            request.OriginalFileName,
                            request.ContentType,
                            request.FileSizeBytes,
                            request.ChecksumAlgorithm,
                            request.ChecksumValue,
                            DateTimeOffset.UtcNow
                        )
                    );
                    return result.Match(
                        model =>
                        {
                            if (model.UploadMode == "ApiProxy")
                                model = model with
                                {
                                    UploadUrl =
                                        $"/lsessions/{sessionId}/evidence-items/{model.EvidenceItemId}/file-upload/content?ownerId={request.OwnerId}&uploadAttemptId={model.UploadAttemptId}",
                                };
                            return Results.Created(
                                $"/lsessions/{sessionId}/evidence-items/{model.EvidenceItemId}/file-upload",
                                model
                            );
                        },
                        ApiResults.Problem
                    );
                }
            )
            .WithTags(Tags.EvidenceItems);
    }

    internal sealed record Request(
        Guid OwnerId,
        string? Caption,
        string OriginalFileName,
        string ContentType,
        long FileSizeBytes,
        string ChecksumAlgorithm,
        string ChecksumValue
    );
}
