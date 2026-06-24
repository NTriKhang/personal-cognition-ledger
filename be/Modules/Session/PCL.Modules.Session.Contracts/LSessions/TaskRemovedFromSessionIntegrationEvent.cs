using Common.Application.EventBus;

namespace PCL.Modules.Session.Contracts.LSessions;

public sealed class TaskRemovedFromSessionIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid sessionId,
    Guid ownerId,
    Guid taskId)
    : IntegrationEvent(id, occurredOnUtc)
{
    public Guid SessionId { get; init; } = sessionId;

    public Guid OwnerId { get; init; } = ownerId;

    public Guid TaskId { get; init; } = taskId;
}
