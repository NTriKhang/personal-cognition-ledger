namespace PCL_API.IntegrationTests.Infrastructure.Messaging;

public sealed record MessageStore
{
    public MessageStore(string moduleName, string schemaName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleName);
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);

        if (!IsValidIdentifier(moduleName) || !IsValidIdentifier(schemaName))
        {
            throw new ArgumentException(
                "Message store names must be SQL-safe identifiers containing ASCII letters, numbers, or underscores.");
        }

        ModuleName = moduleName;
        SchemaName = schemaName;
    }

    public string ModuleName { get; }
    public string SchemaName { get; }

    private static bool IsValidIdentifier(string value) =>
        (IsAsciiLetter(value[0]) || value[0] == '_') &&
        value.Skip(1).All(character =>
            IsAsciiLetter(character) || char.IsAsciiDigit(character) || character == '_');

    private static bool IsAsciiLetter(char character) =>
        character is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
}
