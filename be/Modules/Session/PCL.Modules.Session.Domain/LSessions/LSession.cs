using Common.Domain;
using PCL.Modules.Session.Domain.LSessions.Events;

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
        public Guid OwnerId { get; private set; }
        public int Code { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public DateTimeOffset StartedAt { get; private set; }
        public DateTimeOffset? EndedAt { get; private set; }
        public LSessionStatus Status { get; private set; }

        private readonly List<Guid> _taskIds = new();
        // TODO: Review whether these ids represent planned learning tasks rather than executed activities.
        public IReadOnlyList<Guid> TaskIds => _taskIds.AsReadOnly();

        // For ORM / serializer
        private LSession() { }

        /// <summary>
        /// Factory to create a new LearningSession. Ensures StartedAt is provided and session starts Active.
        /// </summary>
        public static Result<LSession> StartNew(Guid ownerId, string title, DateTimeOffset startedAt, IEnumerable<Guid>? activityIds = null)
        {
            if (ownerId == Guid.Empty)
                return Result.Failure<LSession>(LSessionErrors.InvalidOwnerId);

            if (string.IsNullOrWhiteSpace(title))
                return Result.Failure<LSession>(LSessionErrors.InvalidTitle);

            var session = new LSession
            {
                Id = Guid.NewGuid(),
                OwnerId = ownerId,
                Title = title.Trim(),
                StartedAt = startedAt,
                Status = LSessionStatus.Active
            };

            if (activityIds != null)
            {
                session._taskIds.AddRange(activityIds);
            }

            session.Raise(new LSessionStartedDomainEvent(session.Id, session.StartedAt));

            return Result.Success(session);
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
            Status = LSessionStatus.Stopped;

            Raise(new LSessionStoppedDomainEvent(Id, endedAt));

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
            if (Status != LSessionStatus.Active)
                return Result.Failure(LSessionErrors.NotActive);

            if (activityId == Guid.Empty)
                return Result.Failure(LSessionErrors.InvalidActivityId);

            if (!_taskIds.Contains(activityId))
                _taskIds.Add(activityId);

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
            if (Status != LSessionStatus.Active)
                return Result.Failure(LSessionErrors.NotActive);

            if (activityId == Guid.Empty)
                return Result.Failure(LSessionErrors.InvalidActivityId);

            _taskIds.Remove(activityId);

            return Result.Success();
        }
    }
}
