using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Application.Storage;

public interface IEvidenceFileStorageResolver
{
    IEvidenceFileStorage Resolve(EvidenceStorageProviderType providerType);
}
