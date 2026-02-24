using System.Reflection;

namespace CabaVS.AzureDevOpsHelper.Application;

public static class AssemblyMarker
{
    public static readonly Assembly Application = typeof(AssemblyMarker).Assembly;
}
