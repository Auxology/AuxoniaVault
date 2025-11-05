using Auth.Domain.Aggregates.User;
using Auth.Domain.Constants;
using Auth.Domain.Errors;
using Auth.Domain.Events;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;

namespace Auth.Domain.Entities;

public class EmailChangeRequest : Entity
{
    public int Id { get; private set; }

    public UserId UserId { get; private set; }

    public EmailAddress CurrentEmail { get; private set; }

    public EmailAddress NewEmail { get; private set; }

    public int? CurrentEmailOtp { get; private set; }

    public int? NewEmailOtp { get; private set; }

    public DateTimeOffset RequestedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public EmailChangeMethod Method { get; private set; }

    public EmailChangeStep CurrentStep { get; private set; }

    private EmailChangeRequest()
    {
    }

    private EmailChangeRequest
    (
        UserId userId,
        EmailAddress currentEmail,
        EmailAddress newEmail,
        DateTimeOffset requestedAt,
        DateTimeOffset expiresAt,
        EmailChangeMethod method,
        EmailChangeStep currentStep
    )
    {
        UserId = userId;
        CurrentEmail = currentEmail;
        NewEmail = newEmail;
        RequestedAt = requestedAt;
        ExpiresAt = expiresAt;
        Method = method;
        CurrentStep = currentStep;
    }

    internal static Result<EmailChangeRequest> StartTraditional(UserId userId, EmailAddress currentEmail,
        EmailAddress newEmail, IDateTimeProvider dateTimeProvider)
    {
        if (userId.IsEmpty())
            return Result.Failure<EmailChangeRequest>(EmailChangeRequestErrors.UserIdRequired);

        if (newEmail == currentEmail)
            return Result.Failure<EmailChangeRequest>(EmailChangeRequestErrors.NewCannotBeSameAsCurrent);

        DateTimeOffset utcNow = dateTimeProvider.UtcNow;

        var request = new EmailChangeRequest
        (
            userId,
            currentEmail,
            newEmail,
            utcNow,
            utcNow.AddMinutes(EmailChangeRequestConstants.ExpiresInMinutes),
            EmailChangeMethod.Traditional,
            EmailChangeStep.VerifyCurrent
        );

        return Result.Success(request);
    }

    internal Result SetCurrentEmailOtp(int otp, string ipAddress, string userAgent)
    {
        if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress.Length > RequestMetadataConstants.MaxIpAddressLength)
            return Result.Failure(EmailChangeRequestErrors.InvalidIpAddress);
        
        if (string.IsNullOrWhiteSpace(userAgent) || userAgent.Length > RequestMetadataConstants.MaxUserAgentLength)
            return Result.Failure(EmailChangeRequestErrors.InvalidUserAgent);
        
        if (otp < 100000 || otp > 999999)
            return Result.Failure(EmailChangeRequestErrors.InvalidOtp);

        if (CurrentStep is not EmailChangeStep.VerifyCurrent)
            return Result.Failure(EmailChangeRequestErrors.InvalidStep);

        CurrentEmailOtp = otp;

        Raise(new EmailChangeRequestedDomainEvent
        (
            UserId: UserId.Value,
            CurrentEmail: CurrentEmail.Value,
            CurrentOtp: otp,
            IpAddress: ipAddress,
            UserAgent: userAgent,
            RequestedAt: RequestedAt
        ));

        return Result.Success();
    }

    internal Result VerifyCurrentAndTransitionToVerifyNew(int currentOtp, int newOtp, string ipAddress, string userAgent,
        IDateTimeProvider dateTimeProvider)
    {
        if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress.Length > RequestMetadataConstants.MaxIpAddressLength)
            return Result.Failure(EmailChangeRequestErrors.InvalidIpAddress);
        
        if (string.IsNullOrWhiteSpace(userAgent) || userAgent.Length > RequestMetadataConstants.MaxUserAgentLength)
            return Result.Failure(EmailChangeRequestErrors.InvalidUserAgent);
        
        if (CurrentStep is not EmailChangeStep.VerifyCurrent)
            return Result.Failure<int>(EmailChangeRequestErrors.InvalidStep);

        if (ExpiresAt <= dateTimeProvider.UtcNowForDatabaseComparison() || CurrentEmailOtp != currentOtp)
            return Result.Failure<int>(EmailChangeRequestErrors.InvalidOtp);

        NewEmailOtp = newOtp;

        CurrentStep = EmailChangeStep.VerifyNew;

        Raise(new EmailChangeCurrentEmailVerifiedDomainEvent
        (
            UserId: UserId.Value,
            NewEmail: NewEmail.Value,
            NewOtp: newOtp,
            IpAddress: ipAddress,
            UserAgent: userAgent,
            RequestedAt: RequestedAt
        ));

        return Result.Success();
    }

    internal Result VerifyNewAndComplete(int newOtp, string ipAddress, string userAgent, IDateTimeProvider dateTimeProvider)
    {
        if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress.Length > RequestMetadataConstants.MaxIpAddressLength)
            return Result.Failure(EmailChangeRequestErrors.InvalidIpAddress);
        
        if (string.IsNullOrWhiteSpace(userAgent) || userAgent.Length > RequestMetadataConstants.MaxUserAgentLength)
            return Result.Failure(EmailChangeRequestErrors.InvalidUserAgent);
        
        if (CurrentStep is not EmailChangeStep.VerifyNew)
            return Result.Failure(EmailChangeRequestErrors.InvalidStep);

        if (ExpiresAt <= dateTimeProvider.UtcNowForDatabaseComparison() || NewEmailOtp != newOtp)
            return Result.Failure(EmailChangeRequestErrors.InvalidOtp);

        CurrentStep = EmailChangeStep.Completed;

        Raise(new EmailChangedDomainEvent
        (
            UserId: UserId.Value,
            NewEmail: NewEmail.Value,
            IpAddress: ipAddress,
            UserAgent: userAgent,
            ChangedAt: dateTimeProvider.UtcNow
        ));

        return Result.Success();
    }
}