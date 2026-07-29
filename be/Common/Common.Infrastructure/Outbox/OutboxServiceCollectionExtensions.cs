using Common.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace Common.Infrastructure.Outbox;

public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxProcessor<TModule>(
        this IServiceCollection services,
        IConfigurationSection configurationSection)
        where TModule : IModuleMarker
    {
        string moduleName = TModule.ModuleName;

        services.Configure<OutboxOptions>(moduleName, configurationSection);

        services.AddSingleton<IConfigureOptions<QuartzOptions>>(sp =>
            new ConfigureProcessOutboxJob(
                moduleName,
                sp.GetRequiredService<IOptionsMonitor<OutboxOptions>>()));

        return services;
    }
}
