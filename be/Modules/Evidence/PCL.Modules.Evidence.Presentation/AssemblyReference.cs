using System.Reflection;

namespace PCL.Modules.Evidence.Presentation;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
