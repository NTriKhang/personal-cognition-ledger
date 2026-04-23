using Common.Domain;

namespace PCL.Modules.Session.Domain.LSessions
{
    public sealed class LSessionEndedDomainEvent(Guid sessionId, DateTimeOffset endedAt) : DomainEvent
    {
        public Guid SessionId { get; init; } = sessionId;
        public DateTimeOffset EndedAt { get; init; } = endedAt;
    }
}