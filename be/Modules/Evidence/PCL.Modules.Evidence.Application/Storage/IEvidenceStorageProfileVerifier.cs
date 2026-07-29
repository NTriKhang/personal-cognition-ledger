using Common.Domain;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Application.Storage;

public interface IEvidenceStorageProfileVerifier
{
    EvidenceStorageProviderType ProviderType { get; }

    Task<Result> VerifyAsync(
        StorageProfile storageProfile,
        CancellationToken cancellationToken = default);
}
