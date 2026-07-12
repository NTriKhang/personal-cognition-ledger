using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Evidence.Application.Abstractions.Data;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Domain.EvidenceItems;

namespace PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;
internal sealed class CancelFileUploadCommandHandler(IEvidenceItemRepository items, IUnitOfWork unitOfWork)
    : ICommandHandler<CancelFileUploadCommand, FileUploadReadModel>
{
    public async Task<Result<FileUploadReadModel>> Handle(CancelFileUploadCommand r, CancellationToken ct)
    {
        EvidenceItem? item = await items.GetAsync(EvidenceItemId.From(r.EvidenceItemId), ct);
        Result valid = FileUploadGuard.Validate(item, r.SessionId, r.OwnerId);
        if (valid.IsFailure) return Result.Failure<FileUploadReadModel>(valid.Error);
        Result cancelled = item!.CancelFileUpload(EvidenceFileUploadAttemptId.From(r.UploadAttemptId), r.CancelledAt);
        if (cancelled.IsFailure) return Result.Failure<FileUploadReadModel>(cancelled.Error);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(InitializeFileUploadCommandHandler.ToModel(item, new("Complete", null, "", new Dictionary<string, string>())));
    }
}
