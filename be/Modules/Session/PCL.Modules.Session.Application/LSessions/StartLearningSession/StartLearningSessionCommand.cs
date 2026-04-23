using Common.Application.Messaging;

namespace PCL.Modules.Session.Application.LSessions.StartLearningSession
{
    public record StartLearningSessionCommand(Guid? Id, DateTimeOffset StartedAt, IEnumerable<Guid>? ActivityIds = null) : ICommand<Guid>;
}

