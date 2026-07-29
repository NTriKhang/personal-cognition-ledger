using FluentValidation;

namespace PCL.Modules.TaskPlanning.Application.Tasks.RefineTask
{
    internal sealed class RefineTaskCommandValidator : AbstractValidator<RefineTaskCommand>
    {
        public RefineTaskCommandValidator()
        {
            RuleFor(command => command.TaskId).NotEmpty();
            RuleFor(command => command.OwnerId).NotEmpty();
            RuleFor(command => command.Title).NotEmpty().MaximumLength(200);
            RuleFor(command => command.Description).MaximumLength(4000);
        }
    }
}

