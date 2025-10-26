using Auth.SharedKernel;

namespace Auth.Domain.ValueObjects;

public readonly record struct AuditLogId
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

    private AuditLogId(Guid value)
    {
        Value = value;
    }

    public static AuditLogId New() => new(Guid.NewGuid());

    public static Result<AuditLogId> FromGuid(Guid value)
    {
        if (value == Guid.Empty)
            return Result.Failure<AuditLogId>(Invalid);

        return Result.Success(new AuditLogId(value));
    }

    public static AuditLogId UnsafeFromGuid(Guid value) => new(value);

    public static Result<AuditLogId> FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<AuditLogId>(StringRequired);

        if (!Guid.TryParse(value, out var guid) || guid == Guid.Empty)
            return Result.Failure<AuditLogId>(Invalid);

        return Result.Success(FromGuid(guid).Value);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(AuditLogId loginVerificationId) => loginVerificationId.Value;

    public static implicit operator AuditLogId(Guid value) => FromGuid(value).Value;
}