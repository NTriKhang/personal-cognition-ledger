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
    /// - Contains zero or more assigned Tasks (stored as ids)
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

        private readonly List<SessionTaskAssignment> _taskAssignments = [];
        public IReadOnlyList<Guid> AssignedTaskIds => _taskAssignments.Select(assignment => assignment.TaskId).ToList();

        // For ORM / serializer
        private LSession() { }

        /// <summary>
        /// Factory to create a new LearningSession. Ensures StartedAt is provided and session starts Active.
        /// </summary>
        public static Result<LSession> StartNew(Guid ownerId, string title, DateTimeOffset startedAt)
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
        * Assigns a task to the session if it is valid and not already present.
        *
        * @param taskId
        * @return result indicating success or failure
        */
        public Result AssignTask(Guid taskId, DateTimeOffset assignedAt)
        {
            if (Status != LSessionStatus.Active)
                return Result.Failure(LSessionErrors.NotActive);

            if (taskId == Guid.Empty)
                return Result.Failure(LSessionErrors.InvalidTaskId);

            if (_taskAssignments.Any(assignment => assignment.TaskId == taskId))
                return Result.Failure(LSessionErrors.TaskAlreadyAssigned);

            _taskAssignments.Add(SessionTaskAssignment.Create(Id, taskId, assignedAt));

            Raise(new TaskAssignedToSessionDomainEvent(Id, OwnerId, taskId, assignedAt));

            return Result.Success();
        }

        /**
        * Removes a task assignment from the session if it exists.
        *
        * @param taskId
        * @return result indicating success
        */
        public Result RemoveAssignedTask(Guid taskId)
        {
            if (Status != LSessionStatus.Active)
                return Result.Failure(LSessionErrors.NotActive);

            if (taskId == Guid.Empty)
                return Result.Failure(LSessionErrors.InvalidTaskId);

            SessionTaskAssignment? assignment = _taskAssignments.SingleOrDefault(x => x.TaskId == taskId);

            if (assignment is not null)
            {
                _taskAssignments.Remove(assignment);
            }

            return Result.Success();
        }
    }
}
