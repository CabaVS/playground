using System.Reflection;

namespace CabaVS.AzureDevOpsHelper.Persistence;

public static class AssemblyMarker
{
    public static readonly Assembly Persistence = typeof(AssemblyMarker).Assembly;
}
