using Common.Domain;
using PCL.Modules.Session.Domain.LSessions;

namespace PCL.Modules.Session.Application.LSessions.AssignTaskToSession;

public interface IAssignTaskToSessionPolicy
{
    Task<Result> ValidateAsync(
        LSession session,
        Guid taskId,
        CancellationToken cancellationToken = default);
}
