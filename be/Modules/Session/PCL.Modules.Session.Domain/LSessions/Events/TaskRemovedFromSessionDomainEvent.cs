using Common.Domain;

namespace PCL.Modules.Session.Domain.LSessions.Events;

public sealed class TaskRemovedFromSessionDomainEvent(
    Guid sessionId,
    Guid ownerId,
    Guid taskId) : DomainEvent
{
    public Guid SessionId { get; init; } = sessionId;
    public Guid OwnerId { get; init; } = ownerId;
    public Guid TaskId { get; init; } = taskId;
}
