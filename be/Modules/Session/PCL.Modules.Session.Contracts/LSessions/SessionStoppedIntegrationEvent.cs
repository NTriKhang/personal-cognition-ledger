using Common.Application.EventBus;

namespace PCL.Modules.Session.Contracts.LSessions;

public sealed class SessionStoppedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid sessionId,
    DateTimeOffset stoppedAt)
    : IntegrationEvent(id, occurredOnUtc)
{
    public Guid SessionId { get; init; } = sessionId;

    public DateTimeOffset StoppedAt { get; init; } = stoppedAt;
}
