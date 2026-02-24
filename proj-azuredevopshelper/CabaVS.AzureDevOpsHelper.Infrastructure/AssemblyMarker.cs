using System.Reflection;

namespace CabaVS.AzureDevOpsHelper.Infrastructure;

public static class AssemblyMarker
{
    public static readonly Assembly Infrastructure = typeof(AssemblyMarker).Assembly;
}
