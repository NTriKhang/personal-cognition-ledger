using Microsoft.EntityFrameworkCore;
using PCL.Modules.Session.Application.Repositories;
using PCL.Modules.Session.Domain.LSessions;
using PCL.Modules.Session.Infrastructure.Database;

namespace PCL.Modules.Session.Infrastructure.LSessions
{
    public sealed class LSessionRepository(LSessionDbContext context) : ILSessionRepository
    {
        public void AddAsync(LSession session)
        {
            context.LSessions.Add(session);
        }

        public async Task<LSession?> GetAsync(Guid id)
        {
            return await context.LSessions
                .Include("_taskAssignments")
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> HasActiveSessionAsync(Guid ownerId, CancellationToken cancellationToken = default)
        {
            return await context.LSessions.AnyAsync(
                x => x.OwnerId == ownerId && x.Status == LSessionStatus.Active,
                cancellationToken);
        }

        public async Task<bool> IsTaskAssignedToAnotherActiveSessionAsync(
            Guid taskId,
            Guid currentSessionId,
            Guid ownerId,
            CancellationToken cancellationToken = default)
        {
            return await context.SessionTaskAssignments
                .AnyAsync(
                    assignment =>
                        assignment.TaskId == taskId &&
                        assignment.SessionId != currentSessionId &&
                        context.LSessions.Any(
                            session =>
                                session.Id == assignment.SessionId &&
                                session.OwnerId == ownerId &&
                                session.Status == LSessionStatus.Active),
                    cancellationToken);
        }
    }
}

