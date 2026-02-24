using System.Diagnostics.CodeAnalysis;

namespace CabaVS.Shared.Domain.Primitives;

[SuppressMessage(
    "Major Code Smell",
    "S4035:Classes implementing \"IEquatable<T>\" should be sealed",
    Justification = "Target type is taken into account for that DDD element.")]
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object> GetAtomicValues();

    public static bool operator ==(ValueObject? first, ValueObject? second) =>
        first is not null &&
        first.Equals(second);

    public static bool operator !=(ValueObject? first, ValueObject? second) =>
        !(first == second);

    public bool Equals(ValueObject? other) =>
        other is not null &&
        GetType() == other.GetType() &&
        GetAtomicValues().SequenceEqual(other.GetAtomicValues());

    public override bool Equals(object? obj) =>
        obj is ValueObject other &&
        Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(
            GetAtomicValues().Aggregate(0, HashCode.Combine),
            GetType().GetHashCode()) * 47;
}
