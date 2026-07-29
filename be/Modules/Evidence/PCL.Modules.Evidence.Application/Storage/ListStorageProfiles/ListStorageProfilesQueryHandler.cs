using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Application.Storage.ListStorageProfiles;

internal sealed class ListStorageProfilesQueryHandler(
    IStorageProfileRepository repository)
    : IQueryHandler<ListStorageProfilesQuery, IReadOnlyCollection<StorageProfileReadModel>>
{
    public async Task<Result<IReadOnlyCollection<StorageProfileReadModel>>> Handle(
        ListStorageProfilesQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<StorageProfile> profiles =
            await repository.ListAsync(cancellationToken);

        IReadOnlyCollection<StorageProfileReadModel> readModels = profiles
            .Select(StorageProfileReadModelMapper.Map)
            .ToArray();

        return Result.Success(readModels);
    }
}
