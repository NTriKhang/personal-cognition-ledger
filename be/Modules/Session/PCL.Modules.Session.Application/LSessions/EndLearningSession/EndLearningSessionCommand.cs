using Common.Application.Messaging;

namespace PCL.Modules.Session.Application.LSessions.EndLearningSession
{
    public record EndLearningSessionCommand(Guid Id, DateTimeOffset EndedAt) : ICommand<Guid>;
}