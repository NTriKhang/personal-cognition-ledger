using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Evidence.Application.Storage.CreateS3StorageProfile;

namespace PCL.Modules.Evidence.Presentation.Storage;

internal sealed class CreateS3StorageProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("admin/evidence-storage/profiles/amazon-s3", async (
            CreateS3StorageProfileRequest request,
            ISender sender) =>
        {
            var command = new CreateS3StorageProfileCommand(
                request.Name,
                request.BucketName,
                request.Region,
                request.KeyPrefix,
                DateTimeOffset.UtcNow);

            Result<Guid> result = await sender.Send(command);

            return result.Match(
                id => Results.Created(
                    $"/api/admin/evidence-storage/profiles/{id}",
                    id),
                ApiResults.Problem);
        })
        .WithTags(Tags.EvidenceStorage);
    }

    internal sealed record CreateS3StorageProfileRequest(
        string Name,
        string BucketName,
        string Region,
        string? KeyPrefix);
}
