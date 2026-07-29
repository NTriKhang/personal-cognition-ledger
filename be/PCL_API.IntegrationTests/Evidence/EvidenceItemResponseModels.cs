namespace PCL_API.IntegrationTests.Evidence;

internal sealed class EvidenceItemResponse
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public Guid OwnerId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTimeOffset AddedAt { get; init; }
    public DateTimeOffset? RemovedAt { get; init; }
    public Guid? RemovedBy { get; init; }
    public string? RemovalReason { get; init; }
}
