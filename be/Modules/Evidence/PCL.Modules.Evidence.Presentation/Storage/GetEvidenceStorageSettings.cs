using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Application.Storage.GetEvidenceStorageSettings;

namespace PCL.Modules.Evidence.Presentation.Storage;

internal sealed class GetEvidenceStorageSettings : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/evidence-storage/settings", async (ISender sender) =>
        {
            Result<EvidenceStorageSettingsReadModel> result =
                await sender.Send(new GetEvidenceStorageSettingsQuery());

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.EvidenceStorage);
    }
}
