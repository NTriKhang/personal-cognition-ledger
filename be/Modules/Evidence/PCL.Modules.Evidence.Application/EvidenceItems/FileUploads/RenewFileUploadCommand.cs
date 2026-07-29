using Common.Application.Messaging;

namespace PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

public sealed record RenewFileUploadCommand(Guid SessionId, Guid EvidenceItemId, Guid OwnerId, Guid UploadAttemptId, DateTimeOffset RenewedAt)
    : ICommand<FileUploadReadModel>;
