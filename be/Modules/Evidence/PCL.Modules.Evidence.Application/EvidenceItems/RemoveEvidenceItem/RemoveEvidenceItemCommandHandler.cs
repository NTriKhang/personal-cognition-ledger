using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Evidence.Application.Abstractions.Data;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Domain.EvidenceItems;

namespace PCL.Modules.Evidence.Application.EvidenceItems.RemoveEvidenceItem;

internal sealed class RemoveEvidenceItemCommandHandler(
    IEvidenceItemRepository repository,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveEvidenceItemCommand>
{
    public async Task<Result> Handle(
        RemoveEvidenceItemCommand request,
        CancellationToken cancellationToken)
    {
        EvidenceItemId evidenceItemId = EvidenceItemId.From(request.EvidenceItemId);
        EvidenceItem? evidenceItem = await repository.GetAsync(evidenceItemId, cancellationToken);

        if (evidenceItem is null || evidenceItem.SessionId != request.SessionId)
            return Result.Failure(EvidenceItemErrors.NotFound(evidenceItemId));

        Result removalResult = evidenceItem.Remove(
            request.OwnerId,
            request.RemovedAt,
            request.RemovalReason);

        if (removalResult.IsFailure)
            return removalResult;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
