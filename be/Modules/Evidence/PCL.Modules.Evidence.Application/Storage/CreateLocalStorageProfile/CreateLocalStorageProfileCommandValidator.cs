using FluentValidation;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Application.Storage.CreateLocalStorageProfile;

internal sealed class CreateLocalStorageProfileCommandValidator
    : AbstractValidator<CreateLocalStorageProfileCommand>
{
    public CreateLocalStorageProfileCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(StorageProfile.MaximumNameLength);

        RuleFor(command => command.RootDirectory)
            .NotEmpty()
            .MaximumLength(LocalStorageProfileConfiguration.MaximumRootDirectoryLength);
    }
}
