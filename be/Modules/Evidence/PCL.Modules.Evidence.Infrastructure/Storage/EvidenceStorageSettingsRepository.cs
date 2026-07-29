using Microsoft.EntityFrameworkCore;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Domain.Storage;
using PCL.Modules.Evidence.Infrastructure.Database;

namespace PCL.Modules.Evidence.Infrastructure.Storage;

internal sealed class EvidenceStorageSettingsRepository(EvidenceDbContext context)
    : IEvidenceStorageSettingsRepository
{
    public void Add(EvidenceStorageSettings settings)
    {
        context.StorageSettings.Add(settings);
    }

    public Task<EvidenceStorageSettings?> GetAsync(
        CancellationToken cancellationToken = default)
    {
        return context.StorageSettings.SingleOrDefaultAsync(
            settings => settings.Id == EvidenceStorageSettings.SingletonId,
            cancellationToken);
    }
}
