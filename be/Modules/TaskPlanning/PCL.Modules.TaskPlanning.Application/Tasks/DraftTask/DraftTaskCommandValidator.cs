using FluentValidation;

namespace PCL.Modules.TaskPlanning.Application.Tasks.DraftTask
{
    internal sealed class DraftTaskCommandValidator : AbstractValidator<DraftTaskCommand>
    {
        public DraftTaskCommandValidator()
        {
            RuleFor(command => command.OwnerId)
                .NotEmpty();

            RuleFor(command => command.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(command => command.Description)
                .MaximumLength(4000);
        }
    }
}

