using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Domain.EvidenceItems;

namespace PCL.Modules.Evidence.Application.EvidenceItems.FileUploads;

internal sealed class GetFileUploadQueryHandler(IEvidenceItemRepository items)
    : IQueryHandler<GetFileUploadQuery, FileUploadReadModel>
{
    public async Task<Result<FileUploadReadModel>> Handle(
        GetFileUploadQuery r,
        CancellationToken ct
    )
    {
        EvidenceItem? item = await items.GetAsync(EvidenceItemId.From(r.EvidenceItemId), ct);
        Result valid = FileUploadGuard.Validate(item, r.SessionId, r.OwnerId);
        return valid.IsFailure
            ? Result.Failure<FileUploadReadModel>(valid.Error)
            : Result.Success(
                InitializeFileUploadCommandHandler.ToModel(
                    item!,
                    new("Status", null, "", new Dictionary<string, string>())
                )
            );
    }
}
