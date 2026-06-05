using Common.Application.EventBus;

namespace PCL.Modules.TaskPlanning.Contracts.Tasks;

public sealed class TaskSnapshotChangedIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid taskId,
    int code,
    Guid ownerId,
    string title,
    string? description,
    string status,
    string? category,
    string priority,
    DateTimeOffset createdAt,
    DateTimeOffset? updatedAt,
    DateTimeOffset? plannedAt,
    DateTimeOffset? activatedAt,
    DateTimeOffset? completedAt,
    DateTimeOffset? cancelledAt)
    : IntegrationEvent(id, occurredOnUtc)
{
    public Guid TaskId { get; init; } = taskId;
    public int Code { get; init; } = code;
    public Guid OwnerId { get; init; } = ownerId;
    public string Title { get; init; } = title;
    public string? Description { get; init; } = description;
    public string Status { get; init; } = status;
    public string? Category { get; init; } = category;
    public string Priority { get; init; } = priority;
    public DateTimeOffset CreatedAt { get; init; } = createdAt;
    public DateTimeOffset? UpdatedAt { get; init; } = updatedAt;
    public DateTimeOffset? PlannedAt { get; init; } = plannedAt;
    public DateTimeOffset? ActivatedAt { get; init; } = activatedAt;
    public DateTimeOffset? CompletedAt { get; init; } = completedAt;
    public DateTimeOffset? CancelledAt { get; init; } = cancelledAt;
}
