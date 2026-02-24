using CabaVS.AzureDevOpsHelper.Application.Features.Reports;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CabaVS.AzureDevOpsHelper.Presentation.Endpoints;

internal sealed class RemainingWorkEndpoint(ISender sender) : Endpoint<RemainingWorkEndpoint.EndpointRequest, Ok<RemainingWorkEndpoint.EndpointResponse>>
{
    public override void Configure()
    {
        Get("/api/work-items/{WorkItemId:int}/remaining-work");
        Options(x =>
        {
            x.WithName(nameof(RemainingWorkEndpoint));
            x.WithTags("Reports");
        });

        AllowAnonymous(); // WIP: Remove this when auth is implemented
    }

    public override async Task<Ok<EndpointResponse>> ExecuteAsync(EndpointRequest req, CancellationToken ct)
    {
        var request = new GetRemainingWorkReportQuery(req.WorkItemId);

        Dictionary<string, Dictionary<string, double>> result = await sender.Send(request, ct);

        return TypedResults.Ok(new EndpointResponse(result));
    }

    internal sealed record EndpointRequest(int WorkItemId);
    internal sealed record EndpointResponse(Dictionary<string, Dictionary<string, double>> Report);

    internal sealed class SwaggerSummary : Summary<RemainingWorkEndpoint>
    {
        public SwaggerSummary()
        {
            Summary = "Gets the remaining work report for a given work item.";
            Description = "This endpoint retrieves a report of the remaining work for a specified work item, including details on the remaining work for each child item.";
            
            ExampleRequest = new EndpointRequest(55373);

            Response(
                StatusCodes.Status200OK,
                "The remaining work report was successfully retrieved.",
                example: new EndpointResponse(new Dictionary<string, Dictionary<string, double>>
                {
                    { "Team 1", new Dictionary<string, double> { { "Functional", 50d }, { "Technical", 25d } } },
                    { "Team 2", new Dictionary<string, double> { { "Functional", 200d }, { "Technical", 50d } } }
                }));
        }
    }
}
