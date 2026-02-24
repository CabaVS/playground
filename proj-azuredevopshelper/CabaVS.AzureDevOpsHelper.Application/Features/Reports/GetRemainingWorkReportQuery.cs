using CabaVS.AzureDevOpsHelper.Application.Contracts.Infrastructure;
using MediatR;

namespace CabaVS.AzureDevOpsHelper.Application.Features.Reports;

public sealed record GetRemainingWorkReportQuery(int RootWorkItemId) : IRequest<Dictionary<string, Dictionary<string, double>>>;

internal sealed class GetRemainingWorkReportQueryHandler(
    IAzureDevOpsService azureDevOpsService) : IRequestHandler<GetRemainingWorkReportQuery, Dictionary<string, Dictionary<string, double>>>
{
    public async Task<Dictionary<string, Dictionary<string, double>>> Handle(GetRemainingWorkReportQuery request, CancellationToken cancellationToken) =>
        await azureDevOpsService.BuildRemainingWorkReportByTeamAndActivityType(request.RootWorkItemId, cancellationToken);
}
