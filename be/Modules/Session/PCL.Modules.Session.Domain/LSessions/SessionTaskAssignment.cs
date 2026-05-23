namespace PCL.Modules.Session.Domain.LSessions
{
    public sealed class SessionTaskAssignment
    {
        public Guid SessionId { get; private set; }
        public Guid TaskId { get; private set; }
        public DateTimeOffset AssignedAt { get; private set; }

        private SessionTaskAssignment()
        {
        }

        internal static SessionTaskAssignment Create(Guid sessionId, Guid taskId, DateTimeOffset assignedAt)
        {
            return new SessionTaskAssignment
            {
                SessionId = sessionId,
                TaskId = taskId,
                AssignedAt = assignedAt
            };
        }
    }
}
