using Common.Application.Messaging;
using Common.Domain;
using PCL.Modules.Evidence.Application.Abstractions.Data;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Domain.EvidenceItems;
using PCL.Modules.Session.Contracts.LSessions;

namespace PCL.Modules.Evidence.Application.EvidenceItems.AddEvidenceItem;

internal sealed class AddEvidenceItemCommandHandler(
    ISessionEvidenceAttachmentEligibilityChecker sessionEligibilityChecker,
    IEvidenceItemRepository repository,
    IUnitOfWork unitOfWork) : ICommandHandler<AddEvidenceItemCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        AddEvidenceItemCommand request,
        CancellationToken cancellationToken)
    {
        Result eligibilityResult = await sessionEligibilityChecker.CheckAsync(
            request.SessionId,
            request.OwnerId,
            cancellationToken);

        if (eligibilityResult.IsFailure)
            return Result.Failure<Guid>(eligibilityResult.Error);

        Result<EvidenceItem> registrationResult = EvidenceItem.Register(
            request.SessionId,
            request.OwnerId,
            request.Type,
            request.Content,
            request.AddedAt);

        if (registrationResult.IsFailure)
            return Result.Failure<Guid>(registrationResult.Error);

        EvidenceItem evidenceItem = registrationResult.Value;

        repository.Add(evidenceItem);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(evidenceItem.Id.Value);
    }
}
