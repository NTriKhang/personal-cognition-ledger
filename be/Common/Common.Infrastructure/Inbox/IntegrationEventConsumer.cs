using System.Data.Common;
using Dapper;
using Common.Application.Data;
using Common.Application.EventBus;
using Common.Infrastructure;
using Common.Infrastructure.Serialization;
using MassTransit;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Common.Infrastructure.Inbox;

public sealed class IntegrationEventConsumer<TIntegrationEvent, TModule>(
    IDbConnectionFactory dbConnectionFactory,
    IOptionsMonitor<InboxOptions> inboxOptions)
    : IConsumer<TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
    where TModule : IModuleMarker
{
    public async Task Consume(ConsumeContext<TIntegrationEvent> context)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        TIntegrationEvent integrationEvent = context.Message;

        var inboxMessage = new InboxMessage
        {
            Id = integrationEvent.Id,
            Type = integrationEvent.GetType().Name,
            Content = JsonConvert.SerializeObject(integrationEvent, SerializerSettings.Instance),
            OccurredOnUtc = integrationEvent.OccurredOnUtc
        };

        InboxOptions options = inboxOptions.Get(TModule.ModuleName);

        string sql =
            $"""
            INSERT INTO {options.SchemaName}.inbox_messages("Id", "Type", "Content", "OccurredOnUtc")
            VALUES (@Id, @Type, @Content::jsonb, @OccurredOnUtc)
            """;

        await connection.ExecuteAsync(sql, inboxMessage);
    }
}
