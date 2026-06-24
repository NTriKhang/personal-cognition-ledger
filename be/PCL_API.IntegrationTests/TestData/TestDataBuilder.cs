namespace PCL_API.IntegrationTests.TestData;

public static class TestDataBuilder
{
    public static Guid NewOwnerId() => Guid.NewGuid();

    public static object StartSessionRequest(
        Guid ownerId,
        string title = "Integration test session") =>
        new
        {
            Id = (Guid?)null,
            OwnerId = ownerId,
            Title = title,
            StartedAt = DateTimeOffset.UtcNow
        };

    public static object DraftTaskRequest(
        Guid ownerId,
        string title = "Integration test task") =>
        new
        {
            OwnerId = ownerId,
            Title = title,
            Description = "Created by the API integration test suite.",
            CreatedAt = DateTimeOffset.UtcNow
        };

    public static object AddNoteEvidenceRequest(
        Guid ownerId,
        string content = "Integration test evidence") =>
        new
        {
            OwnerId = ownerId,
            Type = 1,
            Content = content,
            AddedAt = DateTimeOffset.UtcNow
        };
}
