namespace PCL.Modules.Evidence.Infrastructure.Storage.Local;

internal sealed class LocalEvidenceStorageOptions
{
    public const string SectionName = "EvidenceStorage:Local";

    public string[] AllowedRootDirectories { get; init; } = [];
}
