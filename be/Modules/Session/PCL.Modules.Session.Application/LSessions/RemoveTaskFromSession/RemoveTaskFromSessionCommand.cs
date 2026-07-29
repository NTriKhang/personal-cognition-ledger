using Common.Application.Messaging;

namespace PCL.Modules.Session.Application.LSessions.RemoveTaskFromSession
{
    public record RemoveTaskFromSessionCommand(Guid SessionId, Guid TaskId) : ICommand;
}
