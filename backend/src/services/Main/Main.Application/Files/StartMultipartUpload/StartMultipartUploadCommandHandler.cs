using Main.Application.Abstractions.Authentication;
using Main.Application.Abstractions.Database;
using Main.Application.Abstractions.Messaging;
using Main.Application.Abstractions.Storage;
using Main.Application.Errors;
using Main.Domain.Aggregates.Account;
using Main.Domain.ValueObjects;
using Main.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Files.StartMultipartUpload;

internal sealed class StartMultipartUploadCommandHandler(
    IMainDbContext context,
    IUserContext userContext,
    IStorageServices storageServices) : ICommandHandler<StartMultipartUploadCommand, StartMultipartUploadResponse>
{
    public async Task<Result<StartMultipartUploadResponse>> Handle(StartMultipartUploadCommand request, CancellationToken cancellationToken)
    {
        UserId userId = UserId.UnsafeFromGuid(userContext.UserId);

        Account? account = await context.Accounts
            .FirstOrDefaultAsync(a => a.Id == userId, cancellationToken);
        
        if (account is null)
            return Result.Failure<StartMultipartUploadResponse>(AccountErrors.NotFound);
        
        Result<bool> canUpload = account.CanUploadFile(request.FileSize);
        
        if (canUpload.IsFailure)
            return Result.Failure<StartMultipartUploadResponse>(canUpload.Error);
        
        await context.SaveChangesAsync(cancellationToken);
        
        Result<StartS3Response> startResult = await storageServices.StartMultiPartUploadAsync(userId.ToString(),
            request.FileName, request.ContentType, cancellationToken);

        if (startResult.IsFailure)
        {
            return Result.Failure<StartMultipartUploadResponse>(startResult.Error);
        }
        
        var response = new StartMultipartUploadResponse
        (
            FileKey: startResult.Value.FileKey,
            UploadId: startResult.Value.UploadId
        );
        
        return Result.Success(response);
    }
}