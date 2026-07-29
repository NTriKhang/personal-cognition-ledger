using Common.Domain;

namespace PCL.Modules.Session.Domain.LSessions
{
    public static class LSessionErrors
    {
        public static Error NotFound(Guid lsessionId) =>
            Error.NotFound("LSession.NotFound", $"The session with the identifier {lsessionId} was not found");

        public static readonly Error AlreadyEnded =
            Error.Problem(
                "LSession.AlreadyEnded",
                "The learning session has already been ended.");

        public static readonly Error InvalidTitle =
            Error.Problem(
                "LSession.InvalidTitle",
                "The learning session title is required.");

        public static readonly Error InvalidOwnerId =
            Error.Problem(
                "LSession.InvalidOwnerId",
                "OwnerId cannot be empty.");

        public static readonly Error ActiveSessionAlreadyExists =
            Error.Conflict(
                "LSession.ActiveSessionAlreadyExists",
                "Another active learning session already exists.");

        public static readonly Error InvalidEndTime =
            Error.Problem(
                "LSession.InvalidEndTime",
                "EndedAt cannot be before StartedAt.");

        public static readonly Error NotActive =
            Error.Problem(
                "LSession.NotActive",
                "The learning session is not active.");

        public static readonly Error InvalidTaskId =
            Error.Problem(
                "LSession.InvalidTaskId",
                "TaskId cannot be empty.");

        public static readonly Error TaskAlreadyAssigned =
            Error.Conflict(
                "LSession.TaskAlreadyAssigned",
                "The task is already assigned to this learning session.");

        public static readonly Error TaskAlreadyAssignedToAnotherActiveSession =
            Error.Conflict(
                "LSession.TaskAlreadyAssignedToAnotherActiveSession",
                "The task is already assigned to another active learning session.");
    }
}
