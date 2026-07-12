using Common.Application.Messaging;

namespace PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

public sealed record GetFileUploadQuery(Guid SessionId, Guid EvidenceItemId, Guid OwnerId)
    : IQuery<FileUploadReadModel>;
