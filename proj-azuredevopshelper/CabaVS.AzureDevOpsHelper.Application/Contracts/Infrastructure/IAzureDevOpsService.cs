namespace CabaVS.AzureDevOpsHelper.Application.Contracts.Infrastructure;

public interface IAzureDevOpsService
{
    Task<Dictionary<string, Dictionary<string, double>>> BuildRemainingWorkReportByTeamAndActivityType(int rootWorkItemId, CancellationToken cancellationToken);
}
