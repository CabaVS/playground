using CabaVS.Shared.Domain.Common;

namespace CabaVS.Shared.Domain.Errors;

public static class StringErrors
{
    public static Error IsNullOrEmpty(string entity, string property) =>
        new(
            $"{entity}.{property}{nameof(IsNullOrEmpty)}",
            $"Property '{property}' is null or empty.");

    public static Error IsNullOrWhitespace(string entity, string property) =>
        new(
            $"{entity}.{property}{nameof(IsNullOrWhitespace)}",
            $"Property '{property}' is null or whitespace.");

    public static Error IsTooLong(string entity, string property, int maxLength, string? value) =>
        new(
            $"{entity}.{property}{nameof(IsTooLong)}",
            $"Property '{property}' should be shorter than {maxLength}. Actual length is {value?.Length ?? 0}.");

    public static Error IsTooShort(string entity, string property, int minLength, string? value) =>
        new(
            $"{entity}.{property}{nameof(IsTooShort)}",
            $"Property '{property}' should be longer than {minLength}. Actual length is {value?.Length ?? 0}.");
}
