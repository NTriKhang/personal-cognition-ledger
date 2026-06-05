using System.Data.Common;
using Dapper;
using Common.Application.Data;
using Common.Application.Messaging;
using Common.Domain;
using Microsoft.Extensions.Options;

namespace Common.Infrastructure.Outbox;

public sealed class IdempotentDomainEventHandler<TDomainEvent>(
    IDomainEventHandler<TDomainEvent> decorated,
    IDbConnectionFactory dbConnectionFactory,
    IOptionsMonitor<OutboxOptions> outboxOptions,
    IEnumerable<OutboxModuleRegistration> moduleRegistrations)
    : DomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    public override async Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        var outboxMessageConsumer = new OutboxMessageConsumer(domainEvent.Id, decorated.GetType().Name);
        OutboxOptions options = GetOutboxOptions();

        if (await OutboxConsumerExistsAsync(connection, outboxMessageConsumer, options))
        {
            return;
        }

        await decorated.Handle(domainEvent, cancellationToken);

        await InsertOutboxConsumerAsync(connection, outboxMessageConsumer, options);
    }

    private OutboxOptions GetOutboxOptions()
    {
        string handlerAssemblyName = decorated.GetType().Assembly.GetName().Name!;

        foreach (OutboxModuleRegistration moduleRegistration in moduleRegistrations)
        {
            OutboxOptions options = outboxOptions.Get(moduleRegistration.ModuleName);

            if (string.Equals(
                    options.HandlerAssemblyName,
                    handlerAssemblyName,
                    StringComparison.Ordinal))
            {
                return options;
            }
        }

        throw new InvalidOperationException(
            $"Outbox options were not found for domain event handler assembly '{handlerAssemblyName}'.");
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
