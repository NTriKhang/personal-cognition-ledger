using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Evidence.Application.Storage.CreateLocalStorageProfile;

namespace PCL.Modules.Evidence.Presentation.Storage;

internal sealed class CreateLocalStorageProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("admin/evidence-storage/profiles/local", async (
            CreateLocalStorageProfileRequest request,
            ISender sender) =>
        {
            var command = new CreateLocalStorageProfileCommand(
                request.Name,
                request.RootDirectory,
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

    internal sealed record CreateLocalStorageProfileRequest(
        string Name,
        string RootDirectory);
}
