using FluentValidation;

namespace PCL.Modules.TaskPlanning.Application.Tasks.DeferTask
{
    internal sealed class DeferTaskCommandValidator : AbstractValidator<DeferTaskCommand>
    {
        public DeferTaskCommandValidator()
        {
            RuleFor(command => command.TaskId).NotEmpty();
            RuleFor(command => command.OwnerId).NotEmpty();
        }
    }
}

