using Common.Application.Messaging;

namespace PCL.Modules.Session.Application.LSessions.StartLearningSession
{
    public record StartLSessionCommand(Guid? Id, Guid OwnerId, string Title, DateTimeOffset StartedAt, IEnumerable<Guid>? AssignedTaskIds = null) : ICommand<Guid>;
}

