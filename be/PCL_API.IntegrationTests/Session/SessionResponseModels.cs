namespace PCL_API.IntegrationTests.Session;

internal sealed class SessionResponse
{
    public Guid Id { get; init; }
    public Guid OwnerId { get; init; }
    public int Code { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? EndedAt { get; init; }
    public int Status { get; init; }
    public IReadOnlyList<Guid> AssignedTaskIds { get; init; } = [];
}
