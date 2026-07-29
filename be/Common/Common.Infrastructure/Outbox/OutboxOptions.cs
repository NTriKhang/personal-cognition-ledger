namespace Common.Infrastructure.Outbox;

public sealed class OutboxOptions
{
    public string ModuleName { get; init; } = string.Empty;

    public string HandlerAssemblyName { get; init; } = string.Empty;

    public string SchemaName { get; init; } = "events";

    public int IntervalInSeconds { get; init; }

    public int BatchSize { get; init; }
}