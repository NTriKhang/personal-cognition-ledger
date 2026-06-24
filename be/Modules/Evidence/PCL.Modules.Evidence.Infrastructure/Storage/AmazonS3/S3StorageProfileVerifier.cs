using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Common.Domain;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Infrastructure.Storage.AmazonS3;

internal sealed class S3StorageProfileVerifier : IEvidenceStorageProfileVerifier
{
    public EvidenceStorageProviderType ProviderType =>
        EvidenceStorageProviderType.AmazonS3;

    public async Task<Result> VerifyAsync(
        StorageProfile storageProfile,
        CancellationToken cancellationToken = default)
    {
        if (storageProfile.ProviderType != ProviderType ||
            storageProfile.S3Configuration is null)
        {
            return Result.Failure(StorageProfileErrors.InvalidProvider);
        }

        try
        {
            RegionEndpoint region = RegionEndpoint.GetBySystemName(
                storageProfile.S3Configuration.Region);

            using var s3Client = new AmazonS3Client(region);

            var request = new GetBucketLocationRequest
            {
                BucketName = storageProfile.S3Configuration.BucketName
            };

            await s3Client.GetBucketLocationAsync(request, cancellationToken);

            return Result.Success();
        }
        catch (AmazonS3Exception)
        {
            return Result.Failure(StorageProfileErrors.AmazonS3Unavailable);
        }
        catch (AmazonServiceException)
        {
            return Result.Failure(StorageProfileErrors.AmazonS3Unavailable);
        }
        catch (AmazonClientException)
        {
            return Result.Failure(StorageProfileErrors.AmazonS3Unavailable);
        }
    }
}
