using Auth.SharedKernel;

namespace Auth.Domain.ValueObjects;

public readonly record struct LoginVerificationId
{
    private static Error Invalid => Error.Validation(
        "VerificationId.Invalid",
        "VerificationId cannot be an empty GUID."
    );

    private static Error StringRequired => Error.Validation(
        "VerificationId.StringRequired",
        "VerificationId requires a non-empty string that can be parsed as a valid GUID."
    );

    public Guid Value { get; }

    private LoginVerificationId(Guid value)
    {
        Value = value;
    }

    public static LoginVerificationId New() => new(Guid.NewGuid());

    public static Result<LoginVerificationId> FromGuid(Guid value)
    {
        if (value == Guid.Empty)
            return Result.Failure<LoginVerificationId>(Invalid);

        return Result.Success(new LoginVerificationId(value));
    }

    public static LoginVerificationId UnsafeFromGuid(Guid value) => new(value);

    public static Result<LoginVerificationId> FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<LoginVerificationId>(StringRequired);

        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
            return Result.Failure<LoginVerificationId>(Invalid);

        return Result.Success(FromGuid(guid).Value);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(LoginVerificationId loginVerificationId) => loginVerificationId.Value;

    public static implicit operator LoginVerificationId(Guid value) => FromGuid(value).Value;
}