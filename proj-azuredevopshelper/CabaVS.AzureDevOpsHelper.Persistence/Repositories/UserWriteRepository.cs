using CabaVS.AzureDevOpsHelper.Application.Contracts.Persistence.Repositories;
using CabaVS.AzureDevOpsHelper.Domain.Entities;

namespace CabaVS.AzureDevOpsHelper.Persistence.Repositories;

internal sealed class UserWriteRepository(ApplicationDbContext dbContext) : IUserWriteRepository
{
    public void Add(User user) => dbContext.Users.Add(user);
}
