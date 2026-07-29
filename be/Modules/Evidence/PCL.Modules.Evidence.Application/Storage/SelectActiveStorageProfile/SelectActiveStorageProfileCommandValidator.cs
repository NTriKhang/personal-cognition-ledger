using FluentValidation;

namespace PCL.Modules.Evidence.Application.Storage.SelectActiveStorageProfile;

internal sealed class SelectActiveStorageProfileCommandValidator
    : AbstractValidator<SelectActiveStorageProfileCommand>
{
    public SelectActiveStorageProfileCommandValidator()
    {
        RuleFor(command => command.StorageProfileId).NotEmpty();
    }
}
