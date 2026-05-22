using FluentValidation;

namespace PCL.Modules.TaskPlanning.Application.Tasks.PrioritizeTask
{
    internal sealed class PrioritizeTaskCommandValidator : AbstractValidator<PrioritizeTaskCommand>
    {
        public PrioritizeTaskCommandValidator()
        {
            RuleFor(command => command.TaskId).NotEmpty();
            RuleFor(command => command.OwnerId).NotEmpty();
            RuleFor(command => command.Priority).IsInEnum();
        }
    }
}

