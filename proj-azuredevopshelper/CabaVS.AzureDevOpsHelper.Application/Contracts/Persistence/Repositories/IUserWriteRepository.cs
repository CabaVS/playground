using CabaVS.AzureDevOpsHelper.Domain.Entities;

namespace CabaVS.AzureDevOpsHelper.Application.Contracts.Persistence.Repositories;

public interface IUserWriteRepository
{
    void Add(User user);
}
