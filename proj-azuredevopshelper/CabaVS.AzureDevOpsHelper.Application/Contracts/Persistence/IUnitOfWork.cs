using CabaVS.AzureDevOpsHelper.Application.Contracts.Persistence.Repositories;

namespace CabaVS.AzureDevOpsHelper.Application.Contracts.Persistence;

public interface IUnitOfWork
{
    IUserWriteRepository UserWriteRepository { get; }

    Task SaveChanges(CancellationToken cancellationToken = default);
}
