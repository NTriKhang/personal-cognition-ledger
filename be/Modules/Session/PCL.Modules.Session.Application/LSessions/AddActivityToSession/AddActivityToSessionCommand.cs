using ICommand = Common.Application.Messaging.ICommand;

namespace PCL.Modules.Session.Application.LSessions.AddActivityToSession
{
    public record AddActivityToSessionCommand(Guid SessionId, Guid ActivityId) : ICommand;
}

