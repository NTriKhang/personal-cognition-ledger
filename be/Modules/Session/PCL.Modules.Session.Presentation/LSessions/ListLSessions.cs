using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Session.Application.LSessions.GetLearningSession;
using PCL.Modules.Session.Application.LSessions.ListLearningSessions;

namespace PCL.Modules.Session.Presentation.LSessions
{
    internal sealed class ListLearningSessions : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("lsessions", async (ISender sender) =>
            {
                Result<IReadOnlyCollection<LSessionDto>> result =
                    await sender.Send(new ListLearningSessionsQuery());

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.LSessions);
        }
    }
}
