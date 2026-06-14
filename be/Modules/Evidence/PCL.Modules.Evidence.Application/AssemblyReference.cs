using System.Reflection;

namespace PCL.Modules.Evidence.Application;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
