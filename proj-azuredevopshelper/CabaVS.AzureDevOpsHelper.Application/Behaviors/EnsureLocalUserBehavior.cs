using CabaVS.AzureDevOpsHelper.Application.Contracts.Authorization;
using CabaVS.AzureDevOpsHelper.Application.Contracts.Persistence;
using CabaVS.AzureDevOpsHelper.Application.Contracts.Persistence.Repositories;
using CabaVS.AzureDevOpsHelper.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CabaVS.AzureDevOpsHelper.Application.Behaviors;

internal sealed class EnsureLocalUserBehavior<TRequest, TResponse>(
    ILogger<EnsureLocalUserBehavior<TRequest, TResponse>> logger,
    IUserAccessor userAccessor,
    IUserReadRepository userReadRepository,
    IUnitOfWork unitOfWork) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!userAccessor.TryGetCurrentUser(out LocalUserProjection user))
        {
            logger.LogInformation("No local user found for the current request.");
            return await next(cancellationToken);
        }

        var isExists = await userReadRepository.ExistsByExternalId(user.ExternalId, cancellationToken);
        if (isExists)
        {
            logger.LogInformation("Local user with external ID {ExternalId} already exists.", user.ExternalId);
            return await next(cancellationToken);
        }

        var userToCreate = new User(Guid.CreateVersion7(), user.ExternalId);

        unitOfWork.UserWriteRepository.Add(userToCreate);
        await unitOfWork.SaveChanges(cancellationToken);

        logger.LogInformation("Created local user with external ID {ExternalId}.", user.ExternalId);

        return await next(cancellationToken);
    }
}
