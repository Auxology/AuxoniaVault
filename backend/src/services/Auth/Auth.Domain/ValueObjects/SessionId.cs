using Auth.SharedKernel;

namespace Auth.Domain.ValueObjects;

public readonly record struct SessionId
{
    private static Error Invalid => Error.Validation(
        "SessionId.Invalid",
        "SessionId cannot be an empty GUID."
    );

    private static Error StringRequired => Error.Validation(
        "SessionId.StringRequired",
        "SessionId requires a non-empty string that can be parsed as a valid GUID."
    );

    public Guid Value { get; }

    private SessionId(Guid value)
    {
        Value = value;
    }

    public static SessionId New() => new(Guid.NewGuid());

    public static Result<SessionId> FromGuid(Guid value)
    {
        if (value == Guid.Empty)
            return Result.Failure<SessionId>(Invalid);

        return Result.Success(new SessionId(value));
    }

    public static SessionId UnsafeFromGuid(Guid value) => new(value);

    public static Result<SessionId> FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<SessionId>(StringRequired);

        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
            return Result.Failure<SessionId>(Invalid);

        return Result.Success(FromGuid(guid).Value);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(SessionId sessionId) => sessionId.Value;
    public static implicit operator SessionId(Guid value) => FromGuid(value).Value;
}