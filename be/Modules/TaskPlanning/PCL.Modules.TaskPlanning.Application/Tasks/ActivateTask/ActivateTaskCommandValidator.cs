using FluentValidation;

namespace PCL.Modules.TaskPlanning.Application.Tasks.ActivateTask
{
    internal sealed class ActivateTaskCommandValidator : AbstractValidator<ActivateTaskCommand>
    {
        public ActivateTaskCommandValidator()
        {
            RuleFor(command => command.TaskId).NotEmpty();
            RuleFor(command => command.OwnerId).NotEmpty();
        }
    }
}

