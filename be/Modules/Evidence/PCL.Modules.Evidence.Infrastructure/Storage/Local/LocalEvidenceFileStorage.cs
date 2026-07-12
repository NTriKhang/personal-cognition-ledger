using System.Security.Cryptography;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Domain.EvidenceItems;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Infrastructure.Storage.Local;

internal sealed class LocalEvidenceFileStorage : IEvidenceFileStorage
{
    public EvidenceStorageProviderType ProviderType => EvidenceStorageProviderType.Local;

    public Task<FileUploadTarget> CreateUploadTargetAsync(
        StorageProfile profile,
        string objectKey,
        string contentType,
        long size,
        string checksum,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult(
            new FileUploadTarget(
                "ApiProxy",
                null,
                "PUT",
                new Dictionary<string, string>
                {
                    { "Content-Type", contentType },
                    { "X-Upload-Checksum-SHA256", checksum },
                }
            )
        );

    public async Task WriteAsync(
        StorageProfile profile,
        string objectKey,
        Stream content,
        long expectedSize,
        string expectedChecksum,
        CancellationToken cancellationToken
    )
    {
        string path = GetPath(profile, objectKey);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string temp = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            long total = 0;
            string checksum;

            await using (
                var output = new FileStream(
                    temp,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    FileOptions.Asynchronous
                )
            )
            {
                using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
                byte[] buffer = new byte[81920];

                while (true)
                {
                    int read = await content.ReadAsync(buffer, cancellationToken);
                    if (read == 0)
                        break;

                    total += read;
                    if (total > EvidenceFilePolicy.MaximumFileSizeBytes)
                        throw new InvalidDataException("File exceeds the maximum size.");

                    hash.AppendData(buffer, 0, read);
                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }

                await output.FlushAsync(cancellationToken);
                checksum = Convert.ToBase64String(hash.GetHashAndReset());
            }

            if (total != expectedSize || checksum != expectedChecksum)
                throw new InvalidDataException(
                    "File size or checksum does not match the reservation."
                );

            File.Move(temp, path, false);
        }
        finally
        {
            if (File.Exists(temp))
                File.Delete(temp);
        }
    }

    public async Task<StoredFileMetadata?> GetMetadataAsync(
        StorageProfile profile,
        string objectKey,
        CancellationToken cancellationToken
    )
    {
        string path = GetPath(profile, objectKey);
        if (!File.Exists(path))
            return null;
        var info = new FileInfo(path);
        await using var stream = File.OpenRead(path);
        string checksum = Convert.ToBase64String(
            await SHA256.HashDataAsync(stream, cancellationToken)
        );
        return new(info.Length, "application/octet-stream", checksum, info.LastWriteTimeUtc, null);
    }

    public Task<FileDownloadTarget> CreateDownloadTargetAsync(
        StorageProfile profile,
        string objectKey,
        string fileName,
        string contentType,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult(
            new FileDownloadTarget(
                null,
                new FileStream(
                    GetPath(profile, objectKey),
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    81920,
                    FileOptions.Asynchronous
                )
            )
        );

    public Task DeleteAsync(StorageProfile profile, string objectKey, CancellationToken cancellationToken)
    {
        string path = GetPath(profile, objectKey);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private static string GetPath(StorageProfile profile, string objectKey)
    {
        string root = Path.GetFullPath(profile.LocalConfiguration!.RootDirectory);
        string path = Path.GetFullPath(
            Path.Combine(root, objectKey.Replace('/', Path.DirectorySeparatorChar))
        );
        if (
            !path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
        )
            throw new InvalidOperationException("Storage key escaped configured root.");
        return path;
    }
}
