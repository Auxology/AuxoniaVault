using Main.Application.Abstractions.Authentication;
using Main.Application.Abstractions.Database;
using Main.Application.Abstractions.Messaging;
using Main.Application.Abstractions.Storage;
using Main.Application.Errors;
using Main.Domain.Aggregates.Account;
using Main.Domain.ValueObjects;
using Main.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Files.PresignedPartUpload;

internal sealed class PresignedPartUploadCommandHandler(
    IMainDbContext context,
    IUserContext userContext,
    IStorageServices storageServices) : ICommandHandler<PresignedPartUploadCommand, PresignedPartUploadResponse>
{
    public async Task<Result<PresignedPartUploadResponse>> Handle(PresignedPartUploadCommand request, CancellationToken cancellationToken)
    {
        UserId userId = UserId.UnsafeFromGuid(userContext.UserId);

        Account? account = await context.Accounts
            .FirstOrDefaultAsync(a => a.Id == userId, cancellationToken);
        
        if (account is null)
            return Result.Failure<PresignedPartUploadResponse>(AccountErrors.NotFound);
      
        Result<string> presignedResult = await storageServices.GetPresignedUrlAsync
        (
            userId: userId.ToString(),
            fileKey: request.FileKey,
            uploadId: request.UploadId,
            partNumber: request.PartNumber,
            cancellationToken: cancellationToken
        );
        
        if (presignedResult.IsFailure)
            return Result.Failure<PresignedPartUploadResponse>(presignedResult.Error);
        
        PresignedPartUploadResponse response = new
        (
            PresignedUrl: presignedResult.Value,
            PartNumber: request.PartNumber
        );
    
        return Result.Success(response);
    }
}