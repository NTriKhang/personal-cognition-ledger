using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Evidence.Application.Abstractions.Data;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Application.Storage.SelectActiveStorageProfile;

internal sealed class SelectActiveStorageProfileCommandHandler(
    IStorageProfileRepository storageProfileRepository,
    IEvidenceStorageSettingsRepository settingsRepository,
    IEvidenceStorageProfileVerifierResolver verifierResolver,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SelectActiveStorageProfileCommand, StorageProfileReadModel>
{
    public async Task<Result<StorageProfileReadModel>> Handle(
        SelectActiveStorageProfileCommand request,
        CancellationToken cancellationToken)
    {
        StorageProfileId storageProfileId = StorageProfileId.From(request.StorageProfileId);
        StorageProfile? storageProfile = await storageProfileRepository.GetAsync(
            storageProfileId,
            cancellationToken);

        if (storageProfile is null)
        {
            return Result.Failure<StorageProfileReadModel>(
                StorageProfileErrors.NotFound(storageProfileId));
        }

        Result<IEvidenceStorageProfileVerifier> resolverResult =
            verifierResolver.Resolve(storageProfile.ProviderType);

        if (resolverResult.IsFailure)
            return Result.Failure<StorageProfileReadModel>(resolverResult.Error);

        Result verificationResult = await resolverResult.Value.VerifyAsync(
            storageProfile,
            cancellationToken);

        if (verificationResult.IsFailure)
            return Result.Failure<StorageProfileReadModel>(verificationResult.Error);

        EvidenceStorageSettings? settings =
            await settingsRepository.GetAsync(cancellationToken);

        if (settings is null)
        {
            settings = EvidenceStorageSettings.Create(request.SelectedAt);
            settingsRepository.Add(settings);
        }

        Result selectionResult = settings.SelectProfile(
            storageProfile,
            request.SelectedAt);

        if (selectionResult.IsFailure)
            return Result.Failure<StorageProfileReadModel>(selectionResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(StorageProfileReadModelMapper.Map(storageProfile));
    }
}
