using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Evidence.Application.Abstractions.Data;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Application.Storage.CreateS3StorageProfile;

internal sealed class CreateS3StorageProfileCommandHandler(
    IStorageProfileRepository repository,
    IEvidenceStorageProfileVerifierResolver verifierResolver,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateS3StorageProfileCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateS3StorageProfileCommand request,
        CancellationToken cancellationToken)
    {
        if (await repository.NameExistsAsync(request.Name.Trim(), cancellationToken))
            return Result.Failure<Guid>(StorageProfileErrors.ProfileNameAlreadyExists);

        Result<StorageProfile> creationResult = StorageProfile.CreateAmazonS3(
            request.Name,
            request.BucketName,
            request.Region,
            request.KeyPrefix,
            request.CreatedAt);

        if (creationResult.IsFailure)
            return Result.Failure<Guid>(creationResult.Error);

        StorageProfile storageProfile = creationResult.Value;

        Result<IEvidenceStorageProfileVerifier> resolverResult =
            verifierResolver.Resolve(storageProfile.ProviderType);

        if (resolverResult.IsFailure)
            return Result.Failure<Guid>(resolverResult.Error);

        Result verificationResult = await resolverResult.Value.VerifyAsync(
            storageProfile,
            cancellationToken);

        if (verificationResult.IsFailure)
            return Result.Failure<Guid>(verificationResult.Error);

        repository.Add(storageProfile);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(storageProfile.Id.Value);
    }
}
