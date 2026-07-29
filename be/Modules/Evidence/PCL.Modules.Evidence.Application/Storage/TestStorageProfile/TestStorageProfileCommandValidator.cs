using FluentValidation;

namespace PCL.Modules.Evidence.Application.Storage.TestStorageProfile;

internal sealed class TestStorageProfileCommandValidator
    : AbstractValidator<TestStorageProfileCommand>
{
    public TestStorageProfileCommandValidator()
    {
        RuleFor(command => command.StorageProfileId).NotEmpty();
    }
}
