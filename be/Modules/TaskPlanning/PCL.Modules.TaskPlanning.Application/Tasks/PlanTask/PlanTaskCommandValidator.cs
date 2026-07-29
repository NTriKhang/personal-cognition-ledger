using FluentValidation;

namespace PCL.Modules.TaskPlanning.Application.Tasks.PlanTask
{
    internal sealed class PlanTaskCommandValidator : AbstractValidator<PlanTaskCommand>
    {
        public PlanTaskCommandValidator()
        {
            RuleFor(command => command.TaskId).NotEmpty();
            RuleFor(command => command.OwnerId).NotEmpty();
        }
    }
}

