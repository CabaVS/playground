using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace CabaVS.AzureDevOpsHelper.API.Services;

internal interface IAzureDevOpsService
{
    Task<int[]> GetFullHierarchyOf(int workItemId, CancellationToken cancellationToken);
    Task<Dictionary<string, WorkItem[]>> GetWorkItemsGroupedByType(int[] workItemIds, CancellationToken cancellationToken);
}

internal sealed class AzureDevOpsService(
    WorkItemTrackingHttpClient witClient,
    ILogger<AzureDevOpsService> logger) : IAzureDevOpsService
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

    public async Task<Dictionary<string, WorkItem[]>> GetWorkItemsGroupedByType(int[] workItemIds, CancellationToken cancellationToken)
    {
        var workItems = new List<WorkItem>(workItemIds.Length);

        foreach (var batch in workItemIds.Chunk(MaxBatchSize))
        {
            List<WorkItem> fetchedBatch = await witClient.GetWorkItemsAsync(
                batch,
                fields: ["System.Id", "System.Title", "System.State", "System.WorkItemType"],
                cancellationToken: cancellationToken);

            logger.LogInformation(
                "Fetched batch of {Count} work items for IDs: {Ids}",
                batch.Length,
                string.Join(", ", batch));

            workItems.AddRange(fetchedBatch);
        }

        var dictionary = workItems
            .GroupBy(wi => wi.Fields["System.WorkItemType"]?.ToString() ?? string.Empty)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.ToArray());

        logger.LogInformation("Grouped work items into {GroupCount}", string.Join(", ", dictionary.Keys.Select(k => $"'{k}': {dictionary[k].Length}")));

        return dictionary;
    }
}
