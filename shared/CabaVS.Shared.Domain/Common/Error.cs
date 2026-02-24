using System.Diagnostics.CodeAnalysis;

namespace CabaVS.Shared.Domain.Common;

[SuppressMessage(
    "Naming",
    "CA1716:Identifiers should not match keywords",
    Justification = "The default name for Result<T> pattern implementation.")]
public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}
