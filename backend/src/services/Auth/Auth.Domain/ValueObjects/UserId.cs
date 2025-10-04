using Auth.SharedKernel;

namespace Auth.Domain.ValueObjects;

public readonly record struct UserId
{
    private static Error Invalid => Error.Validation(
        "UserId.Invalid",
        "UserId cannot be an empty GUID."
    );

    private static Error StringRequired => Error.Validation(
        "UserId.StringRequired",
        "UserId requires a non-empty string that can be parsed as a valid GUID."
    );

    public Guid Value { get; }

    private UserId(Guid value)
    {
        Value = value;
    }

    public static UserId New() => new(Guid.NewGuid());

    public static Result<UserId> FromGuid(Guid value)
    {
        if (value == Guid.Empty)
            return Result.Failure<UserId>(Invalid);

        return Result.Success(new UserId(value));
    }

    public static UserId UnsafeFromGuid(Guid value) => new(value);

    public static Result<UserId> FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<UserId>(StringRequired);

        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
            return Result.Failure<UserId>(Invalid);

        return Result.Success(FromGuid(guid).Value);
    }

    public override string ToString() => Value.ToString();

    public bool IsEmpty() => Value == Guid.Empty;

    public static implicit operator Guid(UserId userId) => userId.Value;

    public static implicit operator UserId(Guid value) => FromGuid(value).Value;
}