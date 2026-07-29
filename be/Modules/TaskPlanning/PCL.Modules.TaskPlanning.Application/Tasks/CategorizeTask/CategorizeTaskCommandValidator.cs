using FluentValidation;
using PCL.Modules.TaskPlanning.Domain.Tasks;

namespace PCL.Modules.TaskPlanning.Application.Tasks.CategorizeTask
{
    internal sealed class CategorizeTaskCommandValidator : AbstractValidator<CategorizeTaskCommand>
    {
        public CategorizeTaskCommandValidator()
        {
            RuleFor(command => command.TaskId).NotEmpty();
            RuleFor(command => command.OwnerId).NotEmpty();
            RuleFor(command => command.Category).IsInEnum();
        }
    }
}

