using CabaVS.Shared.Domain.Common;
using CabaVS.Shared.Domain.Primitives;

namespace CabaVS.AzureDevOpsHelper.Domain.ValueObjects;

public sealed class TeamName : ValueObject
{
    public const int MaxLength = 20;

    public string Value { get; }

    private TeamName(string value) => Value = value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public static Result<TeamName> Create(string balanceName)
    {
        if (string.IsNullOrWhiteSpace(balanceName))
        {
            return Result.Fail<TeamName>(TeamErrors.NameIsNullOrWhitespace());
        }

        if (balanceName.Length > MaxLength)
        {
            return Result.Fail<TeamName>(TeamErrors.NameIsTooLong(balanceName));
        }

        return Result.Success(new TeamName(balanceName));
    }
}
