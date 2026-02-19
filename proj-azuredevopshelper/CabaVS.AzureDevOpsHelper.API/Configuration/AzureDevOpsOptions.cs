namespace CabaVS.AzureDevOpsHelper.API.Configuration;

internal sealed class AzureDevOpsOptions
{
    public const string SectionName = "AzureDevOps";

    public string OrgUrl { get; set; } = default!;
    public string Pat { get; set; } = default!;
}
