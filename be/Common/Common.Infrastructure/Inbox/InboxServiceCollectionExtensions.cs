using Common.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace Common.Infrastructure.Inbox;

public static class InboxServiceCollectionExtensions
{
    public static IServiceCollection AddInboxProcessor<TModule>(
        this IServiceCollection services,
        IConfigurationSection configurationSection)
        where TModule : IModuleMarker
    {
        string moduleName = TModule.ModuleName;

        services.Configure<InboxOptions>(moduleName, configurationSection);

        services.AddSingleton<IConfigureOptions<QuartzOptions>>(sp =>
            new ConfigureProcessInboxJob(
                moduleName,
                sp.GetRequiredService<IOptionsMonitor<InboxOptions>>()));

        return services;
    }
}
