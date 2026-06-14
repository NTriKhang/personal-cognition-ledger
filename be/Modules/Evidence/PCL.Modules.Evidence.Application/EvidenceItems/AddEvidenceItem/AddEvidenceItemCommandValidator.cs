using FluentValidation;
using PCL.Modules.Evidence.Domain.EvidenceItems;

namespace PCL.Modules.Evidence.Application.EvidenceItems.AddEvidenceItem;

internal sealed class AddEvidenceItemCommandValidator : AbstractValidator<AddEvidenceItemCommand>
{
    public AddEvidenceItemCommandValidator()
    {
        RuleFor(command => command.SessionId).NotEmpty();
        RuleFor(command => command.OwnerId).NotEmpty();
        RuleFor(command => command.Type).IsInEnum();
        RuleFor(command => command.Content)
            .NotEmpty()
            .MaximumLength(EvidenceItem.MaximumContentLength);

        RuleFor(command => command.Content)
            .MaximumLength(EvidenceItem.MaximumReferenceLength)
            .When(command => command.Type is EvidenceItemType.Link or EvidenceItemType.FileReference);
    }
}
