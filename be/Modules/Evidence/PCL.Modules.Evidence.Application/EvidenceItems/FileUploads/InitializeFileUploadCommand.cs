using Common.Application.Messaging;

namespace PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

public sealed record InitializeFileUploadCommand(
    Guid SessionId,
    Guid OwnerId,
    string? Caption,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string ChecksumAlgorithm,
    string ChecksumValue,
    DateTimeOffset AddedAt
) : ICommand<FileUploadReadModel>;
