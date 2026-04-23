using Common.Domain;

namespace Common.Application.Exceptions;

public sealed class YJGlobalException : Exception
{
    public YJGlobalException(string requestName, Error? error = default, Exception? innerException = default)
        : base("Application exception", innerException)
    {
        RequestName = requestName;
        Error = error;
    }

    public string RequestName { get; }

    public Error? Error { get; }
}
