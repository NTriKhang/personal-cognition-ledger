using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Evidence.Application.Storage.TestStorageProfile;

namespace PCL.Modules.Evidence.Presentation.Storage;

internal sealed class TestStorageProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("admin/evidence-storage/profiles/{storageProfileId:guid}/test", async (
            Guid storageProfileId,
            ISender sender) =>
        {
            Result result = await sender.Send(
                new TestStorageProfileCommand(storageProfileId));

            return result.Match(
                () => Results.NoContent(),
                ApiResults.Problem);
        })
        .WithTags(Tags.EvidenceStorage);
    }
}
