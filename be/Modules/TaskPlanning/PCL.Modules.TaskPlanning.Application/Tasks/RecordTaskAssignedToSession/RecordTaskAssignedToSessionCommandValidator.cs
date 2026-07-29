using FluentValidation;

namespace PCL.Modules.TaskPlanning.Application.Tasks.RecordTaskAssignedToSession;

internal sealed class RecordTaskAssignedToSessionCommandValidator
    : AbstractValidator<RecordTaskAssignedToSessionCommand>
{
    public RecordTaskAssignedToSessionCommandValidator()
    {
        RuleFor(command => command.TaskId).NotEmpty();
        RuleFor(command => command.OwnerId).NotEmpty();
        RuleFor(command => command.SessionId).NotEmpty();
    }
}
