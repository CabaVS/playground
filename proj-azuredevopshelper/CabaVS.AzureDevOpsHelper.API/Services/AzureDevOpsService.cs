using CabaVS.AzureDevOpsHelper.API.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;

namespace CabaVS.AzureDevOpsHelper.API.Services;

internal interface IAzureDevOpsService
{
    Task<int[]> GetFullHierarchyOf(int workItemId, CancellationToken cancellationToken);
    Task<WorkItem[]> GetWorkItemsDetails(int[] workItemIds, CancellationToken cancellationToken);
    Dictionary<string, Dictionary<string, double>> CalculateRemainingByTeamAndByActivityType(WorkItem[] workItems);
}

internal sealed class AzureDevOpsService(
    WorkItemTrackingHttpClient witClient,
    ILogger<AzureDevOpsService> logger,
    IOptions<TeamDefinitionOptions> options) : IAzureDevOpsService
{
    private const int MaxBatchSize = 200;

    public async Task<int[]> GetFullHierarchyOf(int workItemId, CancellationToken cancellationToken)
    {
        var wiql = new Wiql
        {
            Query = $@"
            SELECT [System.Id]
            FROM WorkItemLinks
            WHERE
                [Source].[System.Id] = {workItemId}
                AND [System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward'
            MODE (Recursive)"
        };

        logger.LogInformation("Executing WIQL query to fetch work item hierarchy for ID {WorkItemId}", workItemId);

        WorkItemQueryResult queryResult = await witClient.QueryByWiqlAsync(
            wiql,
            cancellationToken: cancellationToken);

        if (queryResult.WorkItemRelations == null)
        {
            logger.LogInformation("No linked work items found for ID {WorkItemId}", workItemId);
            return [workItemId]; // return root only
        }

        var allIds = queryResult.WorkItemRelations
            .Where(r => r.Target != null)
            .Select(r => r.Target.Id)
            .Append(workItemId) // include root
            .Distinct()
            .ToArray();

        logger.LogInformation("Found {Count} linked work items for ID {WorkItemId}", allIds.Length - 1, workItemId);

        return allIds;
    }

    public async Task<WorkItem[]> GetWorkItemsDetails(int[] workItemIds, CancellationToken cancellationToken)
    {
        var workItems = new List<WorkItem>(workItemIds.Length);

        foreach (var batch in workItemIds.Chunk(MaxBatchSize))
        {
            List<WorkItem> fetchedBatch = await witClient.GetWorkItemsAsync(
                batch,
                fields: ["System.Id", "System.Title", "System.State", "System.WorkItemType", "System.AssignedTo", "System.Tags", "Microsoft.VSTS.Scheduling.RemainingWork"],
                cancellationToken: cancellationToken);

            logger.LogInformation(
                "Fetched batch of {Count} work items for IDs: {Ids}",
                batch.Length,
                string.Join(", ", batch));

            workItems.AddRange(fetchedBatch);
        }

        return [.. workItems];
    }

    public Dictionary<string, Dictionary<string, double>> CalculateRemainingByTeamAndByActivityType(WorkItem[] workItems)
    {
        WorkItem[] workItemsToProcess = workItems
            .Where(wi => (wi.Fields["System.State"]?.ToString() ?? string.Empty) is not "Closed" and not "Removed")
            .Where(wi => (wi.Fields["System.WorkItemType"]?.ToString()) is "Task" or "Bug")
            .ToArray();

        logger.LogInformation("Grouping {Count} work items after filtering by state and type", workItemsToProcess.Length);

        Dictionary<string, string> teamDefinitionMap = options.Value.TeamDefinitionMap;

        var unclassifiedTags = new HashSet<string>();
        var unclassifiedTeams = new HashSet<string>();

        var processed = new List<(string TeamLabel, string ActivityType, WorkItem WorkItem)>(workItemsToProcess.Length);
        foreach (WorkItem workItem in workItemsToProcess)
        {
            var alias = workItem.Fields.TryGetValue("System.AssignedTo", out var assignedTo)
                ? (assignedTo as IdentityRef)?.UniqueName?.Split('@')?[0]?.ToUpperInvariant() ?? "Unassigned"
                : "Unassigned";

            var team = teamDefinitionMap.TryGetValue(alias, out var mappedTeam) ? mappedTeam : string.Empty;
            if (string.IsNullOrEmpty(team))
            {
                unclassifiedTeams.Add(alias);
            }

            var teamLabel = string.IsNullOrEmpty(team) ? $"Unknown Team ({alias})" : team;
            string activityType;

            if (workItem.Fields["System.WorkItemType"]?.ToString() is "Bug")
            {
                activityType = "Error";
            }
            else
            {
                var tagsCollection = workItem.Fields.TryGetValue("System.Tags", out var tagsObj) && tagsObj is not null
                    ? tagsObj.ToString()?.Split(';').Select(t => t.Trim()).ToArray() ?? []
                    : [];
                
                activityType = tagsCollection.Contains("Functionality", StringComparer.InvariantCultureIgnoreCase)
                    ? "Functionality"
                    : tagsCollection.Contains("Requirements", StringComparer.InvariantCultureIgnoreCase)
                        ? "Requirements"
                        : tagsCollection.Any(t => t is "Non-functional requirements" or "Refactoring")
                            ? "Technical"
                            : tagsCollection.Contains("Release finalization")
                                ? "Release Finalization"
                                : "Other";

                if (activityType == "Other" && tagsCollection.Length > 0)
                {
                    unclassifiedTags.Add(string.Join(", ", tagsCollection));
                }
            }

            processed.Add((teamLabel, activityType, workItem));
        }

        if (unclassifiedTags.Count > 0)
        {
            logger.LogWarning("Found following unclassified tag combinations that led to 'Other' activity type: {UnclassifiedTags}", string.Join(" | ", unclassifiedTags));
        }

        if (unclassifiedTeams.Count > 0)
        {
            logger.LogWarning("Found following unclassified team aliases: {UnclassifiedTeams}", string.Join(", ", unclassifiedTeams));
        }

        var grouped = processed
            .GroupBy(
                x => x.TeamLabel,
                x => (x.ActivityType, x.WorkItem))
            .OrderBy(g => g.Key) // sort teams alphabetically
            .ToDictionary(
                g => g.Key,
                g => g
                    .GroupBy(x => x.ActivityType, x => x.WorkItem)
                    .OrderBy(gg => gg.Key) // sort activity types alphabetically
                    .ToDictionary(gg => gg.Key, gg => gg.Sum(wi => wi.Fields.TryGetValue("Microsoft.VSTS.Scheduling.RemainingWork", out var remaining) ? (double)remaining : 0)));

        logger.LogInformation("Grouped work items into {TeamCount} teams", grouped.Count);

        return grouped;
    }
}
