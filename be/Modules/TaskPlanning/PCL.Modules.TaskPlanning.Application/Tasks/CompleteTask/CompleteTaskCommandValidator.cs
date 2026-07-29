using FluentValidation;

namespace PCL.Modules.TaskPlanning.Application.Tasks.CompleteTask
{
    internal sealed class CompleteTaskCommandValidator : AbstractValidator<CompleteTaskCommand>
    {
        public CompleteTaskCommandValidator()
        {
            RuleFor(command => command.TaskId).NotEmpty();
            RuleFor(command => command.OwnerId).NotEmpty();
            RuleFor(command => command.CompletionNote).MaximumLength(2000);
        }
    }
}

