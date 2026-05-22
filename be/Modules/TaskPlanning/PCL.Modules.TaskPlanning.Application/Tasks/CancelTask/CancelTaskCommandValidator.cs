using FluentValidation;

namespace PCL.Modules.TaskPlanning.Application.Tasks.CancelTask
{
    internal sealed class CancelTaskCommandValidator : AbstractValidator<CancelTaskCommand>
    {
        public CancelTaskCommandValidator()
        {
            RuleFor(command => command.TaskId).NotEmpty();
            RuleFor(command => command.OwnerId).NotEmpty();
            RuleFor(command => command.CancellationReason).MaximumLength(2000);
        }
    }
}

