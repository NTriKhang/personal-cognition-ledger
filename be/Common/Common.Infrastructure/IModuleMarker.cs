namespace Common.Infrastructure;

public interface IModuleMarker
{
    static abstract string ModuleName { get; }
}
