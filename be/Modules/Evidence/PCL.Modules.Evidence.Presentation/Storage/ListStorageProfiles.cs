using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Application.Storage.ListStorageProfiles;

namespace PCL.Modules.Evidence.Presentation.Storage;

internal sealed class ListStorageProfiles : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/evidence-storage/profiles", async (ISender sender) =>
        {
            Result<IReadOnlyCollection<StorageProfileReadModel>> result =
                await sender.Send(new ListStorageProfilesQuery());

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.EvidenceStorage);
    }
}
