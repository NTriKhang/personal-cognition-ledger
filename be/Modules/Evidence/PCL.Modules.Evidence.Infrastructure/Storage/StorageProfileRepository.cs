using Microsoft.EntityFrameworkCore;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Domain.Storage;
using PCL.Modules.Evidence.Infrastructure.Database;

namespace PCL.Modules.Evidence.Infrastructure.Storage;

internal sealed class StorageProfileRepository(EvidenceDbContext context)
    : IStorageProfileRepository
{
    public void Add(StorageProfile storageProfile)
    {
        context.StorageProfiles.Add(storageProfile);
    }

    public async Task<StorageProfile?> GetAsync(
        StorageProfileId storageProfileId,
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .SingleOrDefaultAsync(
                profile => profile.Id == storageProfileId,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<StorageProfile>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await Query()
            .OrderBy(profile => profile.Name)
            .ToArrayAsync(cancellationToken);
    }

    public Task<bool> NameExistsAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return context.StorageProfiles.AnyAsync(
            profile => profile.Name == name,
            cancellationToken);
    }

    private IQueryable<StorageProfile> Query() =>
        context.StorageProfiles
            .Include(profile => profile.LocalConfiguration)
            .Include(profile => profile.S3Configuration);
}
