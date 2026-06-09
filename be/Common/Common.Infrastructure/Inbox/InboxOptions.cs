namespace Common.Infrastructure.Inbox;

public sealed class InboxOptions
{
    public string ModuleName { get; init; } = string.Empty;

    public string HandlerAssemblyName { get; init; } = string.Empty;

    public string SchemaName { get; init; } = "events";

    public int IntervalInSeconds { get; init; }

    public int BatchSize { get; init; }
}
