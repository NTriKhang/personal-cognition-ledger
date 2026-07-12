using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Infrastructure.Storage.AmazonS3;

internal sealed class S3EvidenceFileStorage : IEvidenceFileStorage
{
    public EvidenceStorageProviderType ProviderType => EvidenceStorageProviderType.AmazonS3;

    public async Task<FileUploadTarget> CreateUploadTargetAsync(
        StorageProfile profile,
        string objectKey,
        string contentType,
        long size,
        string checksum,
        DateTimeOffset expiresAt,
        CancellationToken ct
    )
    {
        using var client = Client(profile);
        var request = new GetPreSignedUrlRequest
        {
            BucketName = profile.S3Configuration!.BucketName,
            Key = Key(profile, objectKey),
            Verb = HttpVerb.PUT,
            Expires = expiresAt.UtcDateTime,
            ContentType = contentType,
        };
        request.Headers["x-amz-checksum-sha256"] = checksum;
        string url = await client.GetPreSignedURLAsync(request);
        return new(
            "Direct",
            url,
            "PUT",
            new Dictionary<string, string>
            {
                { "Content-Type", contentType },
                { "x-amz-checksum-sha256", checksum },
            }
        );
    }

    public Task WriteAsync(
        StorageProfile profile,
        string objectKey,
        Stream content,
        long expectedSize,
        string expectedChecksum,
        CancellationToken ct
    ) => throw new NotSupportedException();

    public async Task<StoredFileMetadata?> GetMetadataAsync(
        StorageProfile profile,
        string objectKey,
        CancellationToken ct
    )
    {
        try
        {
            using var client = Client(profile);
            var response = await client.GetObjectMetadataAsync(
                new GetObjectMetadataRequest
                {
                    BucketName = profile.S3Configuration!.BucketName,
                    Key = Key(profile, objectKey),
                },
                ct
            );
            return new(
                response.ContentLength,
                response.Headers.ContentType,
                response.ChecksumSHA256,
                new DateTimeOffset(response.LastModified ?? DateTime.UtcNow),
                response.VersionId
            );
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<FileDownloadTarget> CreateDownloadTargetAsync(
        StorageProfile profile,
        string objectKey,
        string fileName,
        string contentType,
        CancellationToken ct
    )
    {
        using var client = Client(profile);
        string url = await client.GetPreSignedURLAsync(
            new GetPreSignedUrlRequest
            {
                BucketName = profile.S3Configuration!.BucketName,
                Key = Key(profile, objectKey),
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(5),
                ResponseHeaderOverrides = new ResponseHeaderOverrides
                {
                    ContentType = contentType,
                    ContentDisposition =
                        $"attachment; filename=\"{fileName.Replace("\"", string.Empty)}\"",
                },
            }
        );
        return new(url, null);
    }

    private static AmazonS3Client Client(StorageProfile profile) =>
        new(RegionEndpoint.GetBySystemName(profile.S3Configuration!.Region));

    private static string Key(StorageProfile profile, string objectKey) =>
        string.IsNullOrWhiteSpace(profile.S3Configuration!.KeyPrefix)
            ? objectKey
            : $"{profile.S3Configuration.KeyPrefix}/{objectKey}";
}
