using PCL.Modules.Session.Domain.ValueObjects;

namespace PCL.Modules.Session.Application.LSessions.GetLearningSession
{
    /// <summary>
    /// Data Transfer Object for LearningSession entity.
    /// </summary>
    public sealed class LSessionDto
    {
        public Guid Id { get; init; }
        public int Code { get; init; }
        public DateTimeOffset StartedAt { get; init; }
        public DateTimeOffset? EndedAt { get; init; }
        public SessionStatus Status { get; init; }
        public IReadOnlyList<Guid> LearningActivityIds { get; init; } = Array.Empty<Guid>();

        // Mapping is handled via AutoMapper profile instead of a manual factory.
    }
}

