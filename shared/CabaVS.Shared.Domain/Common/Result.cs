namespace CabaVS.Shared.Domain.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public Error Error => IsFailure
        ? field
        : throw new InvalidOperationException($"Unable to access '{nameof(Error)}' property on successful Result.");

    internal Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid parameters passed into Result's constructor.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result<T> Success<T>(T value) => new(true, Error.None, value);

    public static Result Fail(Error error) => new(false, error);
    public static Result<T> Fail<T>(Error error) => new(false, error, default!);

    public static implicit operator Result(Error error) => Fail(error);
}

public class Result<T> : Result
{
    public T Value => IsSuccess
        ? field
        : throw new InvalidOperationException($"Unable to access '{nameof(Value)}' property on a failed Result.");

    internal Result(bool isSuccess, Error error, T value) : base(isSuccess, error) => Value = value;

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Fail<T>(error);
}
