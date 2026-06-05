using Common.Domain;

namespace PCL.Modules.Session.Domain.LSessions.Events;

public sealed class TaskAssignedToSessionDomainEvent(
    Guid sessionId,
    Guid ownerId,
    Guid taskId,
    DateTimeOffset assignedAt) : DomainEvent
{
    public Guid SessionId { get; init; } = sessionId;
    public Guid OwnerId { get; init; } = ownerId;
    public Guid TaskId { get; init; } = taskId;
    public DateTimeOffset AssignedAt { get; init; } = assignedAt;
}
