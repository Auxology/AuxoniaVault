using Main.Application.Abstractions.Authentication;
using Main.Application.Abstractions.Database;
using Main.Application.Abstractions.Messaging;
using Main.Application.Abstractions.Storage;
using Main.Application.Errors;
using Main.Domain.Aggregates.Account;
using Main.Domain.ValueObjects;
using Main.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Files.CompleteMultipartUpload;

internal sealed class CompleteMultiPartUploadCommandHandler(IMainDbContext context, IUserContext userContext, IStorageServices storageServices) : ICommandHandler<CompleteMultipartUploadCommand, string>
{
    public async Task<Result<string>> Handle(CompleteMultipartUploadCommand request, CancellationToken cancellationToken)
    {
        UserId userId = UserId.UnsafeFromGuid(userContext.UserId);

        Account? account = await context.Accounts
            .FirstOrDefaultAsync(a => a.Id == userId, cancellationToken);
        
        if (account is null)
            return Result.Failure<string>(AccountErrors.NotFound);

        Result<string> completeMultiPartResult = await storageServices.CompleteMultiPartUploadAsync
        (
            fileKey: request.FileKey,
            userId: userId.ToString(),
            uploadId: request.UploadId,
            partETags: request.Parts,
            cancellationToken: cancellationToken
        );
        
        if (completeMultiPartResult.IsFailure)
            return Result.Failure<string>(completeMultiPartResult.Error);
        
        return Result.Success(completeMultiPartResult.Value);
    }
}