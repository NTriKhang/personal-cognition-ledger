using Common.Application.Messaging;

namespace PCL.Modules.Session.Application.LSessions.RemoveActivityFromSession
{
    public record RemoveActivityFromSessionCommand(Guid SessionId, Guid ActivityId) : ICommand;
}