using FluentValidation;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Application.Storage.CreateS3StorageProfile;

internal sealed class CreateS3StorageProfileCommandValidator
    : AbstractValidator<CreateS3StorageProfileCommand>
{
    public CreateS3StorageProfileCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(StorageProfile.MaximumNameLength);

        RuleFor(command => command.BucketName)
            .NotEmpty()
            .MaximumLength(S3StorageProfileConfiguration.MaximumBucketNameLength);

        RuleFor(command => command.Region)
            .NotEmpty()
            .MaximumLength(S3StorageProfileConfiguration.MaximumRegionLength);

        RuleFor(command => command.KeyPrefix)
            .MaximumLength(S3StorageProfileConfiguration.MaximumKeyPrefixLength);
    }
}
