using System.Reflection;

namespace CabaVS.AzureDevOpsHelper.Presentation;

public static class AssemblyMarker
{
    public static readonly Assembly Presentation = typeof(AssemblyMarker).Assembly;
}
