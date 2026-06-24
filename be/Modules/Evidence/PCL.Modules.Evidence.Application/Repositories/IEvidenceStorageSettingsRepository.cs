using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Application.Repositories;

public interface IEvidenceStorageSettingsRepository
{
    void Add(EvidenceStorageSettings settings);

    Task<EvidenceStorageSettings?> GetAsync(
        CancellationToken cancellationToken = default);
}
