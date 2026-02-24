using CabaVS.Shared.Domain.Primitives;

namespace CabaVS.AzureDevOpsHelper.Domain.Entities;

public sealed class User(Guid id, Guid externalId) : Entity(id)
{
    public Guid ExternalId { get; } = externalId;
}
