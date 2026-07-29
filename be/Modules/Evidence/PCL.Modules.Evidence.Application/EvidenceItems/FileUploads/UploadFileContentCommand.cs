using Common.Application.Messaging;

namespace PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

public sealed record UploadFileContentCommand(
    Guid SessionId,
    Guid EvidenceItemId,
    Guid OwnerId,
    Guid UploadAttemptId,
    Stream Content
) : ICommand;
