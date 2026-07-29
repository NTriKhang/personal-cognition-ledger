using Common.Application.EventBus;
using Common.Infrastructure;
using MassTransit;

namespace Common.Infrastructure.Inbox;

public static class InboxRegistrationConfiguratorExtensions
{
    public static void AddInboxConsumer<TIntegrationEvent, TModule>(
        this IRegistrationConfigurator registrationConfigurator)
        where TIntegrationEvent : IntegrationEvent
        where TModule : IModuleMarker
    {
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<TIntegrationEvent, TModule>>();
    }
}
