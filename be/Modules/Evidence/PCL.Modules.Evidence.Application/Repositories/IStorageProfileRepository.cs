using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Application.Repositories;

public interface IStorageProfileRepository
{
    void Add(StorageProfile storageProfile);

    Task<StorageProfile?> GetAsync(
        StorageProfileId storageProfileId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<StorageProfile>> ListAsync(
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        string name,
        CancellationToken cancellationToken = default);
}
