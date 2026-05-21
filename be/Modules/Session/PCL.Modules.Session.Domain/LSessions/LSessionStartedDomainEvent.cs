using Common.Domain;

namespace PCL.Modules.Session.Domain.LSessions
{
    public sealed class LSessionStartedDomainEvent(Guid sessionId, DateTimeOffset startedAt) : DomainEvent
    {
        public Guid SessionId { get; init; } = sessionId;
        public DateTimeOffset StartedAt { get; init; } = startedAt;
    }
}
