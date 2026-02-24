namespace CabaVS.AzureDevOpsHelper.Infrastructure.Configuration;

internal sealed class TeamDefinitionOptions
{
    public const string SectionName = "TeamDefinition";

    public Dictionary<string, string> TeamDefinitionMap { get; init; } = [];
}
