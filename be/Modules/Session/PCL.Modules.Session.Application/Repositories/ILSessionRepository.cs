using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PCL.Modules.Session.Domain.LSessions;

namespace PCL.Modules.Session.Application.Repositories
{
    /// <summary>
    /// Repository interface for <see cref="LearningSession"/> aggregate.
    /// Placed in Application layer so handlers can depend on it and Infrastructure can implement it.
    /// </summary>
    public interface ILSessionRepository
    {
        Task<LSession?> GetAsync(Guid id);
        Task<bool> HasActiveSessionAsync(CancellationToken cancellationToken = default);
        void AddAsync(LSession session);
    }
}

