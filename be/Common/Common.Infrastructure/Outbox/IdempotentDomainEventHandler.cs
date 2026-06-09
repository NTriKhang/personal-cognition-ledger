using System.Data.Common;
using Dapper;
using Common.Application.Data;
using Common.Application.Messaging;
using Common.Domain;
using Common.Infrastructure;
using Microsoft.Extensions.Options;

namespace Common.Infrastructure.Outbox;

public sealed class IdempotentDomainEventHandler<TDomainEvent, TModule>(
    IDomainEventHandler<TDomainEvent> decorated,
    IDbConnectionFactory dbConnectionFactory,
    IOptionsMonitor<OutboxOptions> outboxOptions)
    : DomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
    where TModule : IModuleMarker
{
    public override async Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        var outboxMessageConsumer = new OutboxMessageConsumer(domainEvent.Id, decorated.GetType().Name);
        OutboxOptions options = outboxOptions.Get(TModule.ModuleName);

        if (await OutboxConsumerExistsAsync(connection, outboxMessageConsumer, options))
        {
            return;
        }

        await decorated.Handle(domainEvent, cancellationToken);

        await InsertOutboxConsumerAsync(connection, outboxMessageConsumer, options);
    }

    private static async Task<bool> OutboxConsumerExistsAsync(
        DbConnection dbConnection,
        OutboxMessageConsumer outboxMessageConsumer,
        OutboxOptions options)
    {
        string sql =
            $"""
            SELECT EXISTS(
                SELECT 1
                FROM {options.SchemaName}.outbox_message_consumers
                WHERE "OutboxMessageId" = @OutboxMessageId AND
                      "Name" = @Name
            )
            """;

        return await dbConnection.ExecuteScalarAsync<bool>(sql, outboxMessageConsumer);
    }

    private static async Task InsertOutboxConsumerAsync(
        DbConnection dbConnection,
        OutboxMessageConsumer outboxMessageConsumer,
        OutboxOptions options)
    {
        string sql =
            $"""
            INSERT INTO {options.SchemaName}.outbox_message_consumers("OutboxMessageId", "Name")
            VALUES (@OutboxMessageId, @Name)
            """;

        await dbConnection.ExecuteAsync(sql, outboxMessageConsumer);
    }
}
