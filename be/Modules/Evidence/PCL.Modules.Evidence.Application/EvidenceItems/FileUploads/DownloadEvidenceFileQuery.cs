using Common.Application.Messaging;

namespace PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

public sealed record DownloadEvidenceFileQuery(Guid SessionId, Guid EvidenceItemId, Guid OwnerId)
    : IQuery<FileDownloadReadModel>;
