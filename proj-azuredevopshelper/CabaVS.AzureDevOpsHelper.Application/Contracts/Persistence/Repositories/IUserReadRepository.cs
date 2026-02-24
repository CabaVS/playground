namespace CabaVS.AzureDevOpsHelper.Application.Contracts.Persistence.Repositories;

public interface IUserReadRepository
{
    Task<bool> ExistsByExternalId(Guid externalId, CancellationToken cancellationToken = default);
}
