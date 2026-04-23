using Common.Domain;
using PCL.Modules.Session.Domain.ValueObjects;

namespace PCL.Modules.Session.Domain.LSessions
{
    /// <summary>
    /// Represents an intentional period of learning.
    /// Rules:
    /// - Must have StartedAt
    /// - Starts in Active state
    /// - Can be Ended only once
    /// - Contains zero or more LearningActivities (stored as ids)
    /// </summary>
    public class LSession : Entity
    {
        public Guid Id { get; private set; }
        public int Code { get; private set; }
        public DateTimeOffset StartedAt { get; private set; }
        public DateTimeOffset? EndedAt { get; private set; }
        public SessionStatus Status { get; private set; }

        private readonly List<Guid> _learningActivityIds = new();
        public IReadOnlyList<Guid> LearningActivityIds => _learningActivityIds.AsReadOnly();

        // For ORM / serializer
        private LSession() { }

        /// <summary>
        /// Factory to create a new LearningSession. Ensures StartedAt is provided and session starts Active.
        /// </summary>
        public static LSession StartNew(DateTimeOffset startedAt, IEnumerable<Guid>? activityIds = null)
        {
            var session = new LSession
            {
                Id = Guid.NewGuid(),
                StartedAt = startedAt,
                Status = SessionStatus.Active
            };

            if (activityIds != null)
            {
                session._learningActivityIds.AddRange(activityIds);
            }

            return session;
        }

        /// <summary>
        /// Ends the session. Can only be called once.
        /// </summary>
        public Result End(DateTimeOffset endedAt)
        {
            if (EndedAt != null)
                return Result.Failure(LSessionErrors.AlreadyEnded);

            if (endedAt < StartedAt)
                return Result.Failure(LSessionErrors.InvalidEndTime);

            EndedAt = endedAt;
            Status = SessionStatus.Completed;

            Raise(new LSessionEndedDomainEvent(Id, endedAt));

            return Result.Success();
        }

        /**
        * Adds an activity to the session if it is valid and not already present.
        *
        * @param activityId
        * @return result indicating success or failure
        */
        public Result AddActivity(Guid activityId)
        {
            if (activityId == Guid.Empty)
                return Result.Failure(LSessionErrors.InvalidActivityId);

            if (!_learningActivityIds.Contains(activityId))
                _learningActivityIds.Add(activityId);

            return Result.Success();
        }

        /**
        * Removes an activity from the session if it exists.
        *
        * @param activityId
        * @return result indicating success
        */
        public Result RemoveActivity(Guid activityId)
        {
            _learningActivityIds.Remove(activityId);

            return Result.Success();
        }
    }
}
