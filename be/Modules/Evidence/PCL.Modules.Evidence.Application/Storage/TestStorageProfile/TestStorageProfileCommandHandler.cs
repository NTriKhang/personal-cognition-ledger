using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Application.Storage.TestStorageProfile;

internal sealed class TestStorageProfileCommandHandler(
    IStorageProfileRepository repository,
    IEvidenceStorageProfileVerifierResolver verifierResolver)
    : ICommandHandler<TestStorageProfileCommand>
{
    public async Task<Result> Handle(
        TestStorageProfileCommand request,
        CancellationToken cancellationToken)
    {
        StorageProfileId storageProfileId = StorageProfileId.From(request.StorageProfileId);
        StorageProfile? storageProfile = await repository.GetAsync(
            storageProfileId,
            cancellationToken);

        if (storageProfile is null)
            return Result.Failure(StorageProfileErrors.NotFound(storageProfileId));

        Result<IEvidenceStorageProfileVerifier> resolverResult =
            verifierResolver.Resolve(storageProfile.ProviderType);

        if (resolverResult.IsFailure)
            return Result.Failure(resolverResult.Error);

        return await resolverResult.Value.VerifyAsync(storageProfile, cancellationToken);
    }
}
