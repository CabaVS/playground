using System.Diagnostics;
using CabaVS.Shared.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CabaVS.AzureDevOpsHelper.Application.Behaviors;

internal sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Handling {RequestName} with content: {@Request}", requestName, request);

        var stopwatch = Stopwatch.StartNew();
        TResponse? response = await next(cancellationToken);
        stopwatch.Stop();

        switch (response)
        {
            case Result { IsSuccess: true }:
                logger.LogInformation("Handled {RequestName} successfully in {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);
                break;
            case Result { IsSuccess: false } failedResult:
                logger.LogWarning("Handled {RequestName} with failure in {ElapsedMilliseconds}ms. Error: {@Error}", requestName, stopwatch.ElapsedMilliseconds, failedResult.Error);
                break;
            default:
                logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);
                break;
        }

        return response;
    }
}
