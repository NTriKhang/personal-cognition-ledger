using System;
using System.Collections.Generic;
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
        Task<LSession?> GetByIdAsync(Guid id);
        Task AddAsync(LSession session);
        Task UpdateAsync(LSession session);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<LSession>> ListAsync();
    }
}

