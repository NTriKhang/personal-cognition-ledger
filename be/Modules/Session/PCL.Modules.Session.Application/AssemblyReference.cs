using System.Reflection;

namespace PCL.Modules.Session.Application;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
