using System.Data;
using System.Data.Common;
using System.Reflection;
using Common.Application.Clock;
using Common.Application.Data;
using Common.Application.Messaging;
using Common.Domain;
using Common.Infrastructure.Serialization;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;

namespace Common.Infrastructure.Outbox;

[DisallowConcurrentExecution]
internal sealed class ProcessOutboxJob(
    IDbConnectionFactory dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptionsMonitor<OutboxOptions> outboxOptions,
    ILogger<ProcessOutboxJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        string moduleName = context.MergedJobDataMap.GetString("ModuleName")!;

        OutboxOptions options = outboxOptions.Get(moduleName);

        Assembly handlersAssembly = Assembly.Load(options.HandlerAssemblyName);

        logger.LogInformation("{Module} - Beginning to process outbox messages", options.ModuleName);

        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();

        IReadOnlyList<OutboxMessageResponse> outboxMessages =
            await GetOutboxMessagesAsync(connection, transaction, options);

        foreach (OutboxMessageResponse outboxMessage in outboxMessages)
        {
            Exception? exception = null;

            try
            {
                IDomainEvent domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                    outboxMessage.Content,
                    SerializerSettings.Instance)!;

                using IServiceScope scope = serviceScopeFactory.CreateScope();

                IEnumerable<IDomainEventHandler> domainEventHandlers =
                    DomainEventHandlersFactory.GetHandlers(
                        domainEvent.GetType(),
                        scope.ServiceProvider,
                        handlersAssembly);

                foreach (IDomainEventHandler domainEventHandler in domainEventHandlers)
                {
                    await domainEventHandler.Handle(domainEvent);
                }
            }
            catch (Exception caughtException)
            {
                logger.LogError(
                    caughtException,
                    "{Module} - Exception while processing outbox message {MessageId}",
                    options.ModuleName,
                    outboxMessage.Id);

                exception = caughtException;
            }

            await UpdateOutboxMessageAsync(connection, transaction, outboxMessage, exception, options);
        }

        await transaction.CommitAsync();

        logger.LogInformation("{Module} - Completed processing outbox messages", options.ModuleName);
    }

    private static async Task<IReadOnlyList<OutboxMessageResponse>> GetOutboxMessagesAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        OutboxOptions options)
    {
        string sql =
        $"""
         SELECT
             "Id" AS {nameof(OutboxMessageResponse.Id)},
             "Content" AS {nameof(OutboxMessageResponse.Content)}
         FROM {options.SchemaName}.outbox_messages
         WHERE "ProcessedOnUtc" IS NULL
         ORDER BY "OccurredOnUtc"
         LIMIT {options.BatchSize}
         FOR UPDATE
         """;

        IEnumerable<OutboxMessageResponse> outboxMessages =
            await connection.QueryAsync<OutboxMessageResponse>(
                sql,
                transaction: transaction);

        return outboxMessages.ToList();
    }

    private async Task UpdateOutboxMessageAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        OutboxMessageResponse outboxMessage,
        Exception? exception,
        OutboxOptions options)
    {
        string sql =
        $"""
         UPDATE {options.SchemaName}.outbox_messages
         SET
             "ProcessedOnUtc" = @ProcessedOnUtc,
             "Error" = @Error
         WHERE "Id" = @Id
         """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                outboxMessage.Id,
                ProcessedOnUtc = dateTimeProvider.UtcNow,
                Error = exception?.ToString()
            },
            transaction: transaction);
    }

    internal sealed record OutboxMessageResponse(Guid Id, string Content);
}