using Common.Application.Messaging;

namespace PCL.Modules.Session.Application.LSessions.GetLearningSession
{
    public record GetLearningSessionQuery(Guid Id) : IQuery<LSessionDto>;
}