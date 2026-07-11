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
        string title = "Integration test task",
        string? description = "Created by the API integration test suite.",
        DateTimeOffset? createdAt = null) =>
        new
        {
            OwnerId = ownerId,
            Title = title,
            Description = description,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow
        };

    public static object RefineTaskRequest(
        Guid ownerId,
        string title = "Refined integration test task",
        string? description = "Updated through the API.",
        DateTimeOffset? updatedAt = null) =>
        new { OwnerId = ownerId, Title = title, Description = description, UpdatedAt = updatedAt };

    public static object CategorizeTaskRequest(Guid ownerId, int category = 1) =>
        new { OwnerId = ownerId, Category = category, UpdatedAt = (DateTimeOffset?)null };

    public static object PrioritizeTaskRequest(Guid ownerId, int priority = 2) =>
        new { OwnerId = ownerId, Priority = priority, UpdatedAt = (DateTimeOffset?)null };

    public static object CompleteTaskRequest(
        Guid ownerId,
        DateTimeOffset? completedAt = null,
        string? completionNote = "Completed by an integration test.") =>
        new { OwnerId = ownerId, CompletedAt = completedAt, CompletionNote = completionNote };

    public static object CancelTaskRequest(
        Guid ownerId,
        DateTimeOffset? cancelledAt = null,
        string? cancellationReason = null) =>
        new { OwnerId = ownerId, CancelledAt = cancelledAt, CancellationReason = cancellationReason };

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
