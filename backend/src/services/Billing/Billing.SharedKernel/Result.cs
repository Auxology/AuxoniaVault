namespace Billing.SharedKernel;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot have an error.");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(isSuccess: true, Error.None);

    public static Result<T> Success<T>(T value) => new(value, isSuccess: true, Error.None);

    public static Result Failure(Error error) => new(isSuccess: false, error);

    public static Result<T> Failure<T>(Error error) => new(default, isSuccess: false, error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    public Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value => IsSuccess && _value is not null
        ? _value
        : throw new InvalidOperationException("Cannot access the value of a failure result.");
}