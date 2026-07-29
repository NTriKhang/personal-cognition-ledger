using Common.Domain;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Infrastructure.Storage;

internal sealed class EvidenceStorageProfileVerifierResolver(
    IEnumerable<IEvidenceStorageProfileVerifier> verifiers)
    : IEvidenceStorageProfileVerifierResolver
{
    private readonly IReadOnlyDictionary<
        EvidenceStorageProviderType,
        IEvidenceStorageProfileVerifier> _verifiers = verifiers.ToDictionary(
            verifier => verifier.ProviderType);

    public Result<IEvidenceStorageProfileVerifier> Resolve(
        EvidenceStorageProviderType providerType)
    {
        return _verifiers.TryGetValue(providerType, out IEvidenceStorageProfileVerifier? verifier)
            ? Result.Success(verifier)
            : Result.Failure<IEvidenceStorageProfileVerifier>(
                StorageProfileErrors.ProviderNotRegistered);
    }
}
