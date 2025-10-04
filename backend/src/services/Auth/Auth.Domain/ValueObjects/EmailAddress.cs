using Auth.Domain.Constants;
using Auth.SharedKernel;

namespace Auth.Domain.ValueObjects;

public readonly record struct EmailAddress
{
    private static Error StringRequired => Error.Validation
    (
        "EmailAddress.StringRequired",
        "Email address requires a non-empty string to construct itself."
    );

    private static Error Invalid => Error.Validation
    (
        "EmailAddress.Invalid",
        "Email address is not in a valid format."
    );

    private static Error TooLong => Error.Validation
    (
        "EmailAddress.TooLong",
        $"Email address cannot be longer than {UserConstants.MaxEmailLength} characters."
    );

    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static Result<EmailAddress> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<EmailAddress>(StringRequired);

        email = Normalize(email);

        Result validation = Validate(email);

        if (validation.IsFailure)
            return Result.Failure<EmailAddress>(validation.Error);

        return Result.Success(new EmailAddress(email));
    }

    private static Result Validate(string email)
    {
        if (email.Length > UserConstants.MaxEmailLength)
            return Result.Failure(TooLong);

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);

            return Result.Success();
        }
        catch (FormatException)
        {
            return Result.Failure(Invalid);
        }
    }

    public static EmailAddress UnsafeFromString(string email) => new(email);

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();

    public override string ToString() => Value;
}