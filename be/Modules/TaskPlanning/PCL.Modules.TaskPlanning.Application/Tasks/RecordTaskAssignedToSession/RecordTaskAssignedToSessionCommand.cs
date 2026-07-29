using Common.Application.Messaging;

namespace PCL.Modules.TaskPlanning.Application.Tasks.RecordTaskAssignedToSession;

public sealed record RecordTaskAssignedToSessionCommand(
    Guid TaskId,
    Guid OwnerId,
    Guid SessionId,
    DateTimeOffset AssignedAt) : ICommand;
