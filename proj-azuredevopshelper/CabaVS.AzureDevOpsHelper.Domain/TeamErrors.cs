using CabaVS.AzureDevOpsHelper.Domain.Entities;
using CabaVS.AzureDevOpsHelper.Domain.ValueObjects;
using CabaVS.Shared.Domain.Common;
using CabaVS.Shared.Domain.Errors;

namespace CabaVS.AzureDevOpsHelper.Domain;

public static class TeamErrors
{
    public static Error NameIsNullOrWhitespace() =>
        StringErrors.IsNullOrWhitespace(nameof(Team), nameof(Team.Name));
    public static Error NameIsTooLong(string? value) =>
        StringErrors.IsTooLong(nameof(Team), nameof(Team.Name), TeamName.MaxLength, value);
}
