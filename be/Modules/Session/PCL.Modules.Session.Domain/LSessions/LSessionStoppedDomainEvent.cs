using Common.Domain;

namespace PCL.Modules.Session.Domain.LSessions
{
    public sealed class LSessionStoppedDomainEvent(Guid sessionId, DateTimeOffset stoppedAt) : DomainEvent
    {
        public Guid SessionId { get; init; } = sessionId;
        public DateTimeOffset StoppedAt { get; init; } = stoppedAt;
    }
}
