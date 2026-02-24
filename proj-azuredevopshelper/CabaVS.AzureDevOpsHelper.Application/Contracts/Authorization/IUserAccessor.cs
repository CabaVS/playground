namespace CabaVS.AzureDevOpsHelper.Application.Contracts.Authorization;

public interface IUserAccessor
{
    LocalUserProjection GetCurrentUser();
    bool TryGetCurrentUser(out LocalUserProjection user);
}
