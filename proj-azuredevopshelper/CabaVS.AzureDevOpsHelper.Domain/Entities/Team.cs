using CabaVS.AzureDevOpsHelper.Domain.ValueObjects;
using CabaVS.Shared.Domain.Primitives;

namespace CabaVS.AzureDevOpsHelper.Domain.Entities;

public sealed class Team(Guid id, List<TeamMember> members) : Entity(id)
{
    public TeamName Name { get; set; } = default!;

    private readonly List<TeamMember> _members = members;
    public IReadOnlyCollection<TeamMember> Members => _members;
}
