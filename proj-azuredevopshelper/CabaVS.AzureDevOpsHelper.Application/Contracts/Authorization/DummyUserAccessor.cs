using CabaVS.AzureDevOpsHelper.Domain.Entities;

namespace CabaVS.AzureDevOpsHelper.Application.Contracts.Authorization;

internal sealed class DummyUserAccessor : IUserAccessor
{
    public LocalUserProjection GetCurrentUser() => new(Guid.Empty);

    public bool TryGetCurrentUser(out LocalUserProjection user)
    {
        user = GetCurrentUser();
        return true;
    }
}
