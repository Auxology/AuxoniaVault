using System.Security.Cryptography;
using Auth.Application.Abstractions.Authentication;
using Auth.Application.Abstractions.Database;
using Auth.Application.Abstractions.Messaging;
using Auth.Application.Errors;
using Auth.Domain.Entities;
using Auth.Domain.ValueObjects;
using Auth.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Users.RequestEmailChange;

internal sealed class RequestEmailChangeCommandHandler(IAuthDbContext context, IUserContext userContext, IDateTimeProvider dateTimeProvider) : ICommandHandler<RequestEmailChangeCommand>
{
    public async Task<Result> Handle(RequestEmailChangeCommand request, CancellationToken cancellationToken)
    {
        Guid currentUserId = userContext.UserId;
        
        UserId userId = UserId.UnsafeFromGuid(currentUserId);
        EmailAddress newEmail = EmailAddress.UnsafeFromString(request.Email);
        
        if (await context.Users.AnyAsync(u => u.Email == newEmail, cancellationToken))
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);
        
        var user = await context.Users
            .Include(u => u.EmailChangeRequests)
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);
        
        if (user is null)
            return Result.Failure<string>(UserErrors.UserNotFound);    
        
        int currentEmailOtp = RandomNumberGenerator.GetInt32(100000, 999999);
        
        Result<EmailChangeRequest> requestResult = user.RequestEmailChange
        (
            newEmail,
            currentEmailOtp,
            request.RequestMetadata.IpAddress,
            request.RequestMetadata.UserAgent,
            dateTimeProvider
        );
        
        await context.EmailChangeRequests.AddAsync(requestResult.Value, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}