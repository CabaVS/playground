using CabaVS.AzureDevOpsHelper.Application.Contracts.Persistence;
using CabaVS.AzureDevOpsHelper.Application.Contracts.Persistence.Repositories;
using CabaVS.AzureDevOpsHelper.Persistence.Repositories;

namespace CabaVS.AzureDevOpsHelper.Persistence;

internal sealed class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    public IUserWriteRepository UserWriteRepository => field ??= new UserWriteRepository(dbContext);

    public async Task SaveChanges(CancellationToken cancellationToken = default) => await dbContext.SaveChangesAsync(cancellationToken);
}
