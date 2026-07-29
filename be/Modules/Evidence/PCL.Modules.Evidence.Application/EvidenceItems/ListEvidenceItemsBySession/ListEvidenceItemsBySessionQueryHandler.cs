using System.Data.Common;
using Common.Application.Data;
using Common.Application.Messaging;
using Common.Domain;
using Dapper;

namespace PCL.Modules.Evidence.Application.EvidenceItems.ListEvidenceItemsBySession;

internal sealed class ListEvidenceItemsBySessionQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<ListEvidenceItemsBySessionQuery, IReadOnlyCollection<EvidenceItemReadModel>>
{
    public async Task<Result<IReadOnlyCollection<EvidenceItemReadModel>>> Handle(
        ListEvidenceItemsBySessionQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
        $"""
         SELECT
             "Id" AS {nameof(EvidenceItemReadModel.Id)},
             "SessionId" AS {nameof(EvidenceItemReadModel.SessionId)},
             "OwnerId" AS {nameof(EvidenceItemReadModel.OwnerId)},
             "Type" AS {nameof(EvidenceItemReadModel.Type)},
             "Content" AS {nameof(EvidenceItemReadModel.Content)},
             "AddedAt" AS {nameof(EvidenceItemReadModel.AddedAt)},
             "RemovedAt" AS {nameof(EvidenceItemReadModel.RemovedAt)},
             "RemovedBy" AS {nameof(EvidenceItemReadModel.RemovedBy)},
             "RemovalReason" AS {nameof(EvidenceItemReadModel.RemovalReason)}
         FROM evidence.evidence_item
         WHERE "SessionId" = @SessionId AND "OwnerId" = @OwnerId
           AND (@IncludeRemoved OR "RemovedAt" IS NULL)
         ORDER BY "AddedAt", "Id"
         """;

        List<EvidenceItemReadModel> evidenceItems =
            (await connection.QueryAsync<EvidenceItemReadModel>(
                sql,
                new { request.SessionId, request.OwnerId, request.IncludeRemoved })).AsList();

        return Result.Success<IReadOnlyCollection<EvidenceItemReadModel>>(evidenceItems);
    }
}
