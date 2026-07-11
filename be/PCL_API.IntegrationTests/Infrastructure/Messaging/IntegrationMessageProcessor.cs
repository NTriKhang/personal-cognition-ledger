using Npgsql;
using Quartz;
using Xunit.Sdk;

namespace PCL_API.IntegrationTests.Infrastructure.Messaging;

public sealed class IntegrationMessageProcessor(
    string databaseConnectionString,
    ISchedulerFactory schedulerFactory)
{
    private static readonly TimeSpan ProcessingTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan PollingInterval = TimeSpan.FromMilliseconds(50);

    public async Task ProcessOutboxAsync(
        MessageStore source,
        CancellationToken cancellationToken = default)
    {
        await TriggerJobAsync("Outbox", source.ModuleName, cancellationToken);
        await WaitForCountAsync(
            source.SchemaName,
            "outbox_messages",
            count => count == 0,
            $"{source.ModuleName} outbox processing did not complete.",
            cancellationToken);
        await ThrowIfProcessingFailedAsync(
            source,
            "outbox_messages",
            cancellationToken);
    }

    public Task WaitForPendingInboxAsync(
        MessageStore destination,
        CancellationToken cancellationToken = default) =>
        WaitForCountAsync(
            destination.SchemaName,
            "inbox_messages",
            count => count > 0,
            $"The {destination.ModuleName} inbox did not receive a message.",
            cancellationToken);

    public async Task ProcessInboxAsync(
        MessageStore destination,
        CancellationToken cancellationToken = default)
    {
        await TriggerJobAsync("Inbox", destination.ModuleName, cancellationToken);
        await WaitForCountAsync(
            destination.SchemaName,
            "inbox_messages",
            count => count == 0,
            $"{destination.ModuleName} inbox processing did not complete.",
            cancellationToken);
        await ThrowIfProcessingFailedAsync(
            destination,
            "inbox_messages",
            cancellationToken);
    }

    private async Task TriggerJobAsync(
        string messageKind,
        string moduleName,
        CancellationToken cancellationToken)
    {
        IScheduler scheduler = await schedulerFactory.GetScheduler(cancellationToken);
        var jobKey = new JobKey(
            $"Common.Infrastructure.{messageKind}.Process{messageKind}Job.{moduleName}");
        await scheduler.TriggerJob(jobKey, cancellationToken);
    }

    private async Task WaitForCountAsync(
        string schemaName,
        string tableName,
        Func<long, bool> predicate,
        string timeoutMessage,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(ProcessingTimeout);

        try
        {
            await using var connection = new NpgsqlConnection(databaseConnectionString);
            await connection.OpenAsync(timeout.Token);

            while (true)
            {
                string sql =
                    $"""
                     SELECT COUNT(*)
                     FROM {schemaName}.{tableName}
                     WHERE "ProcessedOnUtc" IS NULL
                     """;
                await using NpgsqlCommand command = new(sql, connection);
                long count = (long)(await command.ExecuteScalarAsync(timeout.Token) ?? 0L);

                if (predicate(count))
                    return;

                await Task.Delay(PollingInterval, timeout.Token);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(timeoutMessage);
        }
    }

    private async Task ThrowIfProcessingFailedAsync(
        MessageStore store,
        string tableName,
        CancellationToken cancellationToken)
    {
        string sql =
            $"""
             SELECT "Error"
             FROM {store.SchemaName}.{tableName}
             WHERE "Error" IS NOT NULL
             LIMIT 1
             """;

        await using var connection = new NpgsqlConnection(databaseConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using NpgsqlCommand command = new(sql, connection);
        object? error = await command.ExecuteScalarAsync(cancellationToken);

        if (error is string message)
        {
            throw new XunitException(
                $"{store.ModuleName} {tableName} processing failed: {message}");
        }
    }
}
