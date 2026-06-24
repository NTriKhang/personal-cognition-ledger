using Common.Domain;
using Microsoft.Extensions.Options;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Infrastructure.Storage.Local;

internal sealed class LocalStorageProfileVerifier(
    IOptions<LocalEvidenceStorageOptions> options)
    : IEvidenceStorageProfileVerifier
{
    private const string ProbeDirectoryName = ".pcl-storage-check";

    public EvidenceStorageProviderType ProviderType =>
        EvidenceStorageProviderType.Local;

    public async Task<Result> VerifyAsync(
        StorageProfile storageProfile,
        CancellationToken cancellationToken = default)
    {
        if (storageProfile.ProviderType != ProviderType ||
            storageProfile.LocalConfiguration is null)
        {
            return Result.Failure(StorageProfileErrors.InvalidProvider);
        }

        try
        {
            string configuredRoot = storageProfile.LocalConfiguration.RootDirectory;

            if (!Path.IsPathFullyQualified(configuredRoot))
                return Result.Failure(StorageProfileErrors.InvalidRootDirectory);

            string rootDirectory = Path.GetFullPath(configuredRoot);

            if (!IsAllowed(rootDirectory))
                return Result.Failure(StorageProfileErrors.LocalRootNotAllowed);

            string probeDirectory = Path.Combine(rootDirectory, ProbeDirectoryName);
            string probeFile = Path.Combine(probeDirectory, $"{Guid.NewGuid():N}.tmp");

            Directory.CreateDirectory(probeDirectory);
            await File.WriteAllTextAsync(
                probeFile,
                "PCL storage verification",
                cancellationToken);
            File.Delete(probeFile);

            return Result.Success();
        }
        catch (Exception exception) when (
            exception is IOException or
            UnauthorizedAccessException or
            NotSupportedException or
            ArgumentException)
        {
            return Result.Failure(StorageProfileErrors.LocalDirectoryUnavailable);
        }
    }

    private bool IsAllowed(string rootDirectory)
    {
        string[] allowedRoots = options.Value.AllowedRootDirectories;

        if (allowedRoots.Length == 0)
            return false;

        return allowedRoots.Any(allowedRoot =>
        {
            string normalizedAllowedRoot = EnsureTrailingSeparator(
                Path.GetFullPath(allowedRoot));
            string normalizedRoot = EnsureTrailingSeparator(rootDirectory);

            return normalizedRoot.StartsWith(
                normalizedAllowedRoot,
                StringComparison.OrdinalIgnoreCase);
        });
    }

    private static string EnsureTrailingSeparator(string path) =>
        Path.EndsInDirectorySeparator(path)
            ? path
            : path + Path.DirectorySeparatorChar;
}
