namespace CabaVS.AzureDevOpsHelper.Domain.Entities;

public sealed class TeamMember
{
    public User User { get; set; } = default!;

    public bool IsAdmin { get; set; }
}
