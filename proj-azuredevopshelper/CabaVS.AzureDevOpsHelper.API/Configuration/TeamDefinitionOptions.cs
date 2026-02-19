namespace CabaVS.AzureDevOpsHelper.API.Configuration;

internal sealed class TeamDefinitionOptions
{
    public const string SectionName = "TeamDefinition";

    public Dictionary<string, string> TeamDefinitionMap { get; init; } = [];
}
