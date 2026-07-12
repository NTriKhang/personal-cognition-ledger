using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Application.Storage;

public interface IEvidenceFileStorage
{
    EvidenceStorageProviderType ProviderType { get; }
    Task<FileUploadTarget> CreateUploadTargetAsync(
        StorageProfile profile,
        string objectKey,
        string contentType,
        long size,
        string checksum,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken
    );
    Task WriteAsync(
        StorageProfile profile,
        string objectKey,
        Stream content,
        long expectedSize,
        string expectedChecksum,
        CancellationToken cancellationToken
    );
    Task<StoredFileMetadata?> GetMetadataAsync(
        StorageProfile profile,
        string objectKey,
        CancellationToken cancellationToken
    );
    Task<FileDownloadTarget> CreateDownloadTargetAsync(
        StorageProfile profile,
        string objectKey,
        string fileName,
        string contentType,
        CancellationToken cancellationToken
    );
    Task DeleteAsync(StorageProfile profile, string objectKey, CancellationToken cancellationToken);
}

public sealed record FileUploadTarget(
    string Mode,
    string? Url,
    string Method,
    IReadOnlyDictionary<string, string> RequiredHeaders
);

public sealed record StoredFileMetadata(
    long Size,
    string ContentType,
    string Checksum,
    DateTimeOffset UploadedAt,
    string? VersionId
);

public sealed record FileDownloadTarget(string? Url, Stream? Content);
