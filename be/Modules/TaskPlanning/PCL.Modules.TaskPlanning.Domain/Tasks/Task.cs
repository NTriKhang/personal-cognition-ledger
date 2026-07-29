using Common.Domain;
using PCL.Modules.TaskPlanning.Domain.Tasks.Events;

namespace PCL.Modules.TaskPlanning.Domain.Tasks
{
    public class Task : Entity
    {
        public TaskId Id { get; private set; }
        public int Code { get; private set; }
        public Guid OwnerId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public TaskStatus Status { get; private set; }
        public TaskCategory? Category { get; private set; }
        public TaskPriority Priority { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }
        public DateTimeOffset? PlannedAt { get; private set; }
        public DateTimeOffset? ActivatedAt { get; private set; }
        public DateTimeOffset? CompletedAt { get; private set; }
        public DateTimeOffset? CancelledAt { get; private set; }
        public string? CompletionNote { get; private set; }
        public string? CancellationReason { get; private set; }

        private Task()
        {
        }

        public static Result<Task> Draft(
            Guid ownerId,
            string title,
            string? description,
            DateTimeOffset createdAt)
        {
            if (ownerId == Guid.Empty)
                return Result.Failure<Task>(TaskErrors.InvalidOwnerId);

            string? normalizedTitle = NormalizeRequired(title);

            if (normalizedTitle is null)
                return Result.Failure<Task>(TaskErrors.InvalidTitle);

            var task = new Task
            {
                Id = TaskId.New(),
                OwnerId = ownerId,
                Title = normalizedTitle,
                Description = NormalizeOptional(description),
                Status = TaskStatus.Draft,
                Priority = TaskPriority.Medium,
                CreatedAt = createdAt
            };

            task.Raise(new TaskDraftedDomainEvent(task.Id, task.OwnerId, task.Title));

            return Result.Success(task);
        }

        public Result Plan(DateTimeOffset plannedAt)
        {
            if (IsTerminal())
                return Result.Failure(TaskErrors.AlreadyTerminal);

            if (Status != TaskStatus.Draft)
                return Result.Failure(TaskErrors.CannotPlan);

            Status = TaskStatus.Planned;
            PlannedAt = plannedAt;
            Touch(plannedAt);

            Raise(new TaskPlannedDomainEvent(Id, plannedAt));

            return Result.Success();
        }

        public Result Refine(string title, string? description, DateTimeOffset updatedAt)
        {
            if (IsTerminal())
                return Result.Failure(TaskErrors.AlreadyTerminal);

            string? normalizedTitle = NormalizeRequired(title);

            if (normalizedTitle is null)
                return Result.Failure(TaskErrors.InvalidTitle);

            Title = normalizedTitle;
            Description = NormalizeOptional(description);
            Touch(updatedAt);

            Raise(new TaskRefinedDomainEvent(Id, Title));

            return Result.Success();
        }

        public Result Categorize(TaskCategory category, DateTimeOffset updatedAt)
        {
            if (IsTerminal())
                return Result.Failure(TaskErrors.AlreadyTerminal);

            Category = category;
            Touch(updatedAt);

            Raise(new TaskCategoryChangedDomainEvent(Id, category));

            return Result.Success();
        }

        public Result Prioritize(TaskPriority priority, DateTimeOffset updatedAt)
        {
            if (IsTerminal())
                return Result.Failure(TaskErrors.AlreadyTerminal);

            Priority = priority;
            Touch(updatedAt);

            Raise(new TaskPriorityChangedDomainEvent(Id, priority));

            return Result.Success();
        }

        public Result Activate(DateTimeOffset activatedAt)
        {
            if (IsTerminal())
                return Result.Failure(TaskErrors.AlreadyTerminal);

            if (Status != TaskStatus.Planned)
                return Result.Failure(TaskErrors.CannotActivate);

            Status = TaskStatus.Active;
            ActivatedAt = activatedAt;
            Touch(activatedAt);

            Raise(new TaskActivatedDomainEvent(Id, activatedAt));

            return Result.Success();
        }

        public Result Defer(DateTimeOffset deferredAt)
        {
            if (IsTerminal())
                return Result.Failure(TaskErrors.AlreadyTerminal);

            if (Status != TaskStatus.Active)
                return Result.Failure(TaskErrors.CannotDefer);

            Status = TaskStatus.Planned;
            Touch(deferredAt);

            Raise(new TaskDeferredDomainEvent(Id, deferredAt));

            return Result.Success();
        }

        public Result Complete(DateTimeOffset completedAt, string? completionNote)
        {
            if (IsTerminal())
                return Result.Failure(TaskErrors.AlreadyTerminal);

            if (Status is not TaskStatus.Planned and not TaskStatus.Active)
                return Result.Failure(TaskErrors.CannotComplete);

            if (completedAt < CreatedAt)
                return Result.Failure(TaskErrors.InvalidCompletionTime);

            Status = TaskStatus.Completed;
            CompletedAt = completedAt;
            CompletionNote = NormalizeOptional(completionNote);
            Touch(completedAt);

            Raise(new TaskCompletedDomainEvent(Id, completedAt));

            return Result.Success();
        }

        public Result Cancel(DateTimeOffset cancelledAt, string? cancellationReason)
        {
            if (IsTerminal())
                return Result.Failure(TaskErrors.AlreadyTerminal);

            if (Status is not TaskStatus.Draft and not TaskStatus.Planned and not TaskStatus.Active)
                return Result.Failure(TaskErrors.CannotCancel);

            if (cancelledAt < CreatedAt)
                return Result.Failure(TaskErrors.InvalidCancellationTime);

            Status = TaskStatus.Cancelled;
            CancelledAt = cancelledAt;
            CancellationReason = NormalizeOptional(cancellationReason);
            Touch(cancelledAt);

            Raise(new TaskCancelledDomainEvent(Id, cancelledAt, CancellationReason));

            return Result.Success();
        }

        public bool IsTerminal() =>
            Status is TaskStatus.Completed or TaskStatus.Cancelled;

        private void Touch(DateTimeOffset updatedAt)
        {
            UpdatedAt = updatedAt;
        }

        private static string? NormalizeRequired(string value)
        {
            string normalized = value.Trim();

            return string.IsNullOrWhiteSpace(normalized)
                ? null
                : normalized;
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
