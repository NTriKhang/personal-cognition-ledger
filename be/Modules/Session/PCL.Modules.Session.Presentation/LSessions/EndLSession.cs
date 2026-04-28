using Common.Domain;
using Common.Presentation.Endpoints;
using Common.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using PCL.Modules.Session.Application.LSessions.EndLearningSession;

namespace PCL.Modules.Session.Presentation.LSessions
{
    internal class EndLSession : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("learningsessions/{id:guid}/end", async (Guid id, EndRequest request, ISender sender) =>
            {
                var command = new EndLSessionCommand(id, request.EndedAt);

                Result result = await sender.Send(command);

                return result.Match(
                    () => Results.Ok(),
                    ApiResults.Problem);
            })
            .WithTags("LearningSessions");
        }

        internal sealed record EndRequest(DateTimeOffset EndedAt);
    }
}
