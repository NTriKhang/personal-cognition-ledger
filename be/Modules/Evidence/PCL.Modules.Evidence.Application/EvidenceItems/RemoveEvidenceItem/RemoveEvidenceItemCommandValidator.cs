using FluentValidation;
using PCL.Modules.Evidence.Domain.EvidenceItems;

namespace PCL.Modules.Evidence.Application.EvidenceItems.RemoveEvidenceItem;

internal sealed class RemoveEvidenceItemCommandValidator : AbstractValidator<RemoveEvidenceItemCommand>
{
    public RemoveEvidenceItemCommandValidator()
    {
        RuleFor(command => command.SessionId).NotEmpty();
        RuleFor(command => command.EvidenceItemId).NotEmpty();
        RuleFor(command => command.OwnerId).NotEmpty();
        RuleFor(command => command.RemovalReason)
            .MaximumLength(EvidenceItemErrors.MaximumRemovalReasonLength);
    }
}
