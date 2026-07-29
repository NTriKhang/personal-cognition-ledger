using Common.Domain;

namespace PCL.Modules.Evidence.Domain.Storage;

public static class StorageProfileErrors
{
    public static readonly Error InvalidName =
        Error.Problem(
            "StorageProfile.InvalidName",
            $"Storage profile name is required and cannot exceed {StorageProfile.MaximumNameLength} characters.");

    public static readonly Error InvalidProvider =
        Error.Problem(
            "StorageProfile.InvalidProvider",
            "The storage provider is invalid.");

    public static readonly Error InvalidRootDirectory =
        Error.Problem(
            "StorageProfile.InvalidRootDirectory",
            "A valid absolute root directory is required for Local storage.");

    public static readonly Error InvalidBucketName =
        Error.Problem(
            "StorageProfile.InvalidBucketName",
            "An Amazon S3 bucket name is required.");

    public static readonly Error InvalidRegion =
        Error.Problem(
            "StorageProfile.InvalidRegion",
            "An Amazon S3 region is required.");

    public static readonly Error InvalidKeyPrefix =
        Error.Problem(
            "StorageProfile.InvalidKeyPrefix",
            "The Amazon S3 key prefix is invalid.");

    public static readonly Error ProfileDisabled =
        Error.Conflict(
            "StorageProfile.Disabled",
            "A disabled storage profile cannot be selected.");

    public static readonly Error ProfileNameAlreadyExists =
        Error.Conflict(
            "StorageProfile.NameAlreadyExists",
            "A storage profile with the same name already exists.");

    public static readonly Error StorageNotConfigured =
        Error.Conflict(
            "StorageProfile.NotConfigured",
            "No active evidence storage profile has been configured.");

    public static readonly Error ProviderNotRegistered =
        Error.Problem(
            "StorageProfile.ProviderNotRegistered",
            "The configured storage provider is not available.");

    public static readonly Error LocalRootNotAllowed =
        Error.Problem(
            "StorageProfile.LocalRootNotAllowed",
            "The local root directory is outside the deployment's allowed storage roots.");

    public static readonly Error LocalDirectoryUnavailable =
        Error.Problem(
            "StorageProfile.LocalDirectoryUnavailable",
            "The local storage directory could not be created or written.");

    public static readonly Error AmazonS3Unavailable =
        Error.Problem(
            "StorageProfile.AmazonS3Unavailable",
            "The Amazon S3 bucket could not be accessed with the deployment's AWS identity.");

    public static Error NotFound(StorageProfileId storageProfileId) =>
        Error.NotFound(
            "StorageProfile.NotFound",
            $"Storage profile '{storageProfileId.Value}' was not found.");
}
