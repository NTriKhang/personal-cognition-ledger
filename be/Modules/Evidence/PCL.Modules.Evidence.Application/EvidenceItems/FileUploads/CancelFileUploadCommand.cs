using Common.Application.Messaging;
namespace PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;
public sealed record CancelFileUploadCommand(Guid SessionId, Guid EvidenceItemId, Guid OwnerId, Guid UploadAttemptId, DateTimeOffset CancelledAt)
    : ICommand<FileUploadReadModel>;
