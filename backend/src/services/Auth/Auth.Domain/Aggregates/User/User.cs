using Auth.Domain.Constants;
using Auth.Domain.Entities;
using Auth.Domain.Errors;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;

namespace Auth.Domain.Aggregates.User;

public class User : Entity, IAggregateRoot
{
    public UserId Id { get; private set; }

    public string Name { get; private set; }

    public EmailAddress Email { get; private set; }

    public string? Avatar { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public virtual ICollection<EmailChangeRequest> EmailChangeRequests { get; private set; }

    private User() { } // For EF Core

    private User(string name, EmailAddress email, DateTimeOffset utcNow)
    {
        Id = UserId.New();
        Name = name;
        Email = email;
        CreatedAt = utcNow;
        Avatar = null;
        UpdatedAt = null;
        EmailChangeRequests = [];
    }

    public static Result<User> Create(string name, EmailAddress email, IDateTimeProvider dateTimeProvider)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<User>(UserErrors.NameRequired);

        if (name.Length > UserConstants.MaxNameLength)
            return Result.Failure<User>(UserErrors.NameTooLong);

        DateTimeOffset utcNow = dateTimeProvider.UtcNow;

        var user = new User(name, email, utcNow);

        return Result.Success(user);
    }

    public Result ChangeName(string newName, IDateTimeProvider dateTimeProvider)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure(UserErrors.NameRequired);

        if (newName.Length > UserConstants.MaxNameLength)
            return Result.Failure(UserErrors.NameTooLong);

        DateTimeOffset utcNow = dateTimeProvider.UtcNow;

        Name = newName;
        UpdatedAt = utcNow;

        return Result.Success();
    }

    public Result SetProfilePicture(string avatarKey, IDateTimeProvider dateTimeProvider)
    {
        if (string.IsNullOrWhiteSpace(avatarKey))
            return Result.Failure(UserErrors.AvatarKeyRequired);

        DateTimeOffset utcNow = dateTimeProvider.UtcNow;

        Avatar = avatarKey;
        UpdatedAt = utcNow;

        return Result.Success();
    }

    public Result<EmailChangeRequest> RequestEmailChange(EmailAddress newEmail, int currentOtp, string ipAddress, string userAgent, IDateTimeProvider dateTimeProvider)
    {
        var activeRequest = EmailChangeRequests
            .FirstOrDefault(r => r.CurrentStep != EmailChangeStep.Completed && r.ExpiresAt > dateTimeProvider.UtcNow);

        if (activeRequest != null)
            return Result.Failure<EmailChangeRequest>(EmailChangeRequestErrors.ActiveRequestExists);

        var emailChangeRequestResult = EmailChangeRequest.StartTraditional(Id, Email, newEmail, dateTimeProvider);

        if (emailChangeRequestResult.IsFailure)
            return Result.Failure<EmailChangeRequest>(emailChangeRequestResult.Error);

        var setOtpResult = emailChangeRequestResult.Value.SetCurrentEmailOtp(currentOtp, ipAddress, userAgent);

        if (setOtpResult.IsFailure)
            return Result.Failure<EmailChangeRequest>(setOtpResult.Error);

        EmailChangeRequests.Add(emailChangeRequestResult.Value);

        return emailChangeRequestResult;
    }

    public Result VerifyCurrentEmail(int currentOtp, int newEmailOtp, IDateTimeProvider dateTimeProvider)
    {
        var activeRequest = EmailChangeRequests
            .FirstOrDefault(r =>
                r.CurrentStep == EmailChangeStep.VerifyCurrent &&
                r.ExpiresAt > dateTimeProvider.UtcNow);

        if (activeRequest is null)
            return Result.Failure(EmailChangeRequestErrors.NotFound);

        return activeRequest.VerifyCurrentAndTransitionToVerifyNew(currentOtp, newEmailOtp, dateTimeProvider);
    }

    public Result VerifyNewEmail(int newOtp, IDateTimeProvider dateTimeProvider)
    {
        var activeRequest = EmailChangeRequests
            .FirstOrDefault(r =>
                r.CurrentStep == EmailChangeStep.VerifyNew &&
                r.ExpiresAt > dateTimeProvider.UtcNow);

        if (activeRequest is null)
            return Result.Failure(EmailChangeRequestErrors.NotFound);

        var verifyResult = activeRequest.VerifyNewAndComplete(newOtp, dateTimeProvider);

        if (verifyResult.IsFailure)
            return verifyResult;

        return CompleteEmailChange(activeRequest.NewEmail, dateTimeProvider);
    }

    public Result CompleteEmailChange(EmailAddress newEmail, IDateTimeProvider dateTimeProvider)
    {
        if (newEmail == Email)
            return Result.Failure(UserErrors.EmailCannotBeSame);

        Email = newEmail;
        UpdatedAt = dateTimeProvider.UtcNow;

        return Result.Success();
    }
}
