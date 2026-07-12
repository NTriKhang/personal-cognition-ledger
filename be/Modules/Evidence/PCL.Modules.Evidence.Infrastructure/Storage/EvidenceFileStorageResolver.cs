using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Infrastructure.Storage;

internal sealed class EvidenceFileStorageResolver(IEnumerable<IEvidenceFileStorage> providers)
    : IEvidenceFileStorageResolver
{
    public IEvidenceFileStorage Resolve(EvidenceStorageProviderType providerType) =>
        providers.Single(provider => provider.ProviderType == providerType);
}
