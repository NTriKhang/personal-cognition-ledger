using ICommand = Common.Application.Messaging.ICommand;

namespace PCL.Modules.Session.Application.LSessions.AssignTaskToSession
{
    public record AssignTaskToSessionCommand(Guid SessionId, Guid TaskId, DateTimeOffset AssignedAt) : ICommand;
}

