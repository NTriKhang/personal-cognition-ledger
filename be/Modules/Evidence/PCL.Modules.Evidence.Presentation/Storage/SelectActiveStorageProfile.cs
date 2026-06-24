using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Application.Storage.SelectActiveStorageProfile;

namespace PCL.Modules.Evidence.Presentation.Storage;

internal sealed class SelectActiveStorageProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("admin/evidence-storage/settings/active-profile", async (
            SelectActiveStorageProfileRequest request,
            ISender sender) =>
        {
            var command = new SelectActiveStorageProfileCommand(
                request.StorageProfileId,
                DateTimeOffset.UtcNow);

            Result<StorageProfileReadModel> result = await sender.Send(command);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.EvidenceStorage);
    }

    internal sealed record SelectActiveStorageProfileRequest(
        Guid StorageProfileId);
}
