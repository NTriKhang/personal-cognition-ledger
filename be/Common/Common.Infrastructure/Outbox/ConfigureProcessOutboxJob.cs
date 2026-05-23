using Microsoft.Extensions.Options;
using Quartz;

namespace Common.Infrastructure.Outbox;

public sealed class ConfigureProcessOutboxJob(
    string moduleName,
    IOptionsMonitor<OutboxOptions> outboxOptions)
    : IConfigureOptions<QuartzOptions>
{
    public void Configure(QuartzOptions options)
    {
        OutboxOptions moduleOptions = outboxOptions.Get(moduleName);

        string jobName = $"{typeof(ProcessOutboxJob).FullName}.{moduleOptions.ModuleName}";
        string triggerName = $"{jobName}.Trigger";

        options
            .AddJob<ProcessOutboxJob>(configure =>
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
