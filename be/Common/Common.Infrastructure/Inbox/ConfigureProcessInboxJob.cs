using Common.Infrastructure.Inbox;
using Microsoft.Extensions.Options;
using Quartz;

namespace Common.Infrastructure.Inbox;

public sealed class ConfigureProcessInboxJob(
    string moduleName,
    IOptionsMonitor<InboxOptions> inboxOptions)
    : IConfigureOptions<QuartzOptions>
{
    public void Configure(QuartzOptions options)
    {
        InboxOptions moduleOptions = inboxOptions.Get(moduleName);

        string jobName = $"{typeof(ProcessInboxJob).FullName}.{moduleOptions.ModuleName}";
        string triggerName = $"{jobName}.Trigger";

        options
            .AddJob<ProcessInboxJob>(configure =>
                configure
                    .WithIdentity(jobName)
                    .UsingJobData("ModuleName", moduleOptions.ModuleName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithIdentity(triggerName)
                    .WithSimpleSchedule(schedule =>
                        schedule.WithIntervalInSeconds(moduleOptions.IntervalInSeconds).RepeatForever()));
    }
}
