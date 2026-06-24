using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Application.Storage.GetEvidenceStorageSettings;

internal sealed class GetEvidenceStorageSettingsQueryHandler(
    IStorageProfileRepository storageProfileRepository,
    IEvidenceStorageSettingsRepository settingsRepository)
    : IQueryHandler<GetEvidenceStorageSettingsQuery, EvidenceStorageSettingsReadModel>
{
    public async Task<Result<EvidenceStorageSettingsReadModel>> Handle(
        GetEvidenceStorageSettingsQuery request,
        CancellationToken cancellationToken)
    {
        EvidenceStorageSettings? settings =
            await settingsRepository.GetAsync(cancellationToken);

        if (settings?.ActiveStorageProfileId is not StorageProfileId storageProfileId)
        {
            return Result.Success(
                new EvidenceStorageSettingsReadModel(null, settings?.UpdatedAt));
        }

        StorageProfile? storageProfile = await storageProfileRepository.GetAsync(
            storageProfileId,
            cancellationToken);

        StorageProfileReadModel? activeProfile = storageProfile is null
            ? null
            : StorageProfileReadModelMapper.Map(storageProfile);

        return Result.Success(
            new EvidenceStorageSettingsReadModel(activeProfile, settings.UpdatedAt));
    }
}
