using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace Common.Infrastructure.Outbox;

public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxProcessor(
        this IServiceCollection services,
        string moduleName,
        IConfigurationSection configurationSection)
    {
        services.Configure<OutboxOptions>(moduleName, configurationSection);

        services.AddSingleton<IConfigureOptions<QuartzOptions>>(sp =>
            new ConfigureProcessOutboxJob(
                moduleName,
                sp.GetRequiredService<IOptionsMonitor<OutboxOptions>>()));

        return services;
    }
}