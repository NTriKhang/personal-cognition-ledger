using Common.Application.Messaging;

namespace PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

public sealed record ConfirmFileUploadCommand(
    Guid SessionId,
    Guid EvidenceItemId,
    Guid OwnerId,
    Guid UploadAttemptId,
    DateTimeOffset ConfirmedAt
) : ICommand<FileUploadReadModel>;
