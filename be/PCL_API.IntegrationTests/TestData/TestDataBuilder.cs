namespace PCL_API.IntegrationTests.TestData;

public static class TestDataBuilder
{
    public static Guid NewOwnerId() => Guid.NewGuid();

    public static object StartSessionRequest(
        Guid ownerId,
        string title = "Integration test session",
        DateTimeOffset? startedAt = null) =>
        new
        {
            Id = (Guid?)null,
            OwnerId = ownerId,
            Title = title,
            StartedAt = startedAt ?? DateTimeOffset.UtcNow
        };

    public static object EndSessionRequest(DateTimeOffset? endedAt = null) =>
        new { EndedAt = endedAt ?? DateTimeOffset.UtcNow };

    public static object AssignTaskRequest(DateTimeOffset? assignedAt = null) =>
        new { AssignedAt = assignedAt };

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
        string content = "Integration test evidence",
        DateTimeOffset? addedAt = null) =>
        new
        {
            OwnerId = ownerId,
            Type = 1,
            Content = content,
            AddedAt = addedAt ?? DateTimeOffset.UtcNow
        };

    public static object AddLinkEvidenceRequest(
        Guid ownerId,
        string content = "https://example.com/evidence",
        DateTimeOffset? addedAt = null) =>
        new { OwnerId = ownerId, Type = 2, Content = content, AddedAt = addedAt ?? DateTimeOffset.UtcNow };

    public static object RemoveEvidenceRequest(
        Guid ownerId,
        DateTimeOffset? removedAt = null,
        string? removalReason = null) =>
        new { OwnerId = ownerId, RemovedAt = removedAt, RemovalReason = removalReason };

    public static object CreateLocalStorageProfileRequest(
        string rootDirectory,
        string name = "Integration Local Storage") =>
        new { Name = name, RootDirectory = rootDirectory };

    public static object CreateS3StorageProfileRequest(
        string name = "Integration S3 Storage",
        string bucketName = "pcl-integration-tests",
        string region = "ap-southeast-1",
        string? keyPrefix = "evidence/tests") =>
        new { Name = name, BucketName = bucketName, Region = region, KeyPrefix = keyPrefix };

    public static object SelectActiveStorageProfileRequest(Guid storageProfileId) =>
        new { StorageProfileId = storageProfileId };
}
