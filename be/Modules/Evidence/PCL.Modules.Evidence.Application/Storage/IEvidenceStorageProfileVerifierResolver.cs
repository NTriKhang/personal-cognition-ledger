using Common.Domain;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Application.Storage;

public interface IEvidenceStorageProfileVerifierResolver
{
    Result<IEvidenceStorageProfileVerifier> Resolve(
        EvidenceStorageProviderType providerType);
}
