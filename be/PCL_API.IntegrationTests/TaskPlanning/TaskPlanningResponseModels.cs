namespace PCL_API.IntegrationTests.TaskPlanning;

internal sealed class TaskDetailResponse
{
    public Guid Id { get; init; }
    public int Code { get; init; }
    public Guid OwnerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Category { get; init; }
    public string Priority { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public DateTimeOffset? PlannedAt { get; init; }
    public DateTimeOffset? ActivatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public DateTimeOffset? CancelledAt { get; init; }
    public string? CompletionNote { get; init; }
    public string? CancellationReason { get; init; }
}

internal sealed class TaskSummaryResponse
{
    public Guid Id { get; init; }
    public Guid OwnerId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? Category { get; init; }
    public string Priority { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}
