using Common.Application.Messaging;

namespace PCL.Modules.Session.Application.LSessions.StartLearningSession
{
    public record StartLSessionCommand(Guid? Id, string Title, DateTimeOffset StartedAt, IEnumerable<Guid>? ActivityIds = null) : ICommand<Guid>;
}

