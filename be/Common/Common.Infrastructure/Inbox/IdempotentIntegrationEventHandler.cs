using System.Data.Common;
using Common.Application.Data;
using Common.Application.EventBus;
using Common.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;

namespace Common.Infrastructure.Inbox;

public sealed class IdempotentIntegrationEventHandler<TIntegrationEvent, TModule>(
    IIntegrationEventHandler<TIntegrationEvent> decorated,
    IDbConnectionFactory dbConnectionFactory,
    IOptionsMonitor<InboxOptions> inboxOptions)
    : IntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
    where TModule : IModuleMarker
{
    public override async Task Handle(
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        var inboxMessageConsumer = new InboxMessageConsumer(integrationEvent.Id, decorated.GetType().Name);
        InboxOptions options = inboxOptions.Get(TModule.ModuleName);

        if (await InboxConsumerExistsAsync(connection, inboxMessageConsumer, options))
        {
            return;
        }

        await decorated.Handle(integrationEvent, cancellationToken);

        await InsertInboxConsumerAsync(connection, inboxMessageConsumer, options);
    }

    private static async Task<bool> InboxConsumerExistsAsync(
        DbConnection dbConnection,
        InboxMessageConsumer inboxMessageConsumer,
        InboxOptions options)
    {
        string sql =
            $"""
            SELECT EXISTS(
                SELECT 1
                FROM {options.SchemaName}.inbox_message_consumers
                WHERE "InboxMessageId" = @InboxMessageId AND
                      "Name" = @Name
            )
            """;

        return await dbConnection.ExecuteScalarAsync<bool>(sql, inboxMessageConsumer);
    }

    private static async Task InsertInboxConsumerAsync(
        DbConnection dbConnection,
        InboxMessageConsumer inboxMessageConsumer,
        InboxOptions options)
    {
        string sql =
            $"""
            INSERT INTO {options.SchemaName}.inbox_message_consumers("InboxMessageId", "Name")
            VALUES (@InboxMessageId, @Name)
            """;

        await dbConnection.ExecuteAsync(sql, inboxMessageConsumer);
    }
}
