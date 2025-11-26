using Main.Application.Abstractions.Authentication;
using Main.Application.Abstractions.Database;
using Main.Application.Abstractions.Messaging;
using Main.Application.Abstractions.Storage;
using Main.Application.Errors;
using Main.Domain.Aggregates.Account;
using Main.Domain.Aggregates.FileMetadata;
using Main.Domain.ValueObjects;
using Main.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Files.DownloadFile;

internal sealed class DownloadFileCommandHandler(IMainDbContext context, IUserContext userContext, IStorageServices storageServices) : ICommandHandler<DownloadFileCommand, string>
{
    public async Task<Result<string>> Handle(DownloadFileCommand request, CancellationToken cancellationToken)
    {
        UserId userId = UserId.UnsafeFromGuid(userContext.UserId);
        
        Account? account = await context.Accounts
            .FirstOrDefaultAsync(a => a.Id == userId, cancellationToken);

        if (account is null)
            return Result.Failure<string>(AccountErrors.NotFound);
        
        FileMetadataId fileId = FileMetadataId.UnsafeFromGuid(request.FileId);
        
        FileMetadata? file = await context.Files
            .FirstOrDefaultAsync(f => f.Id == fileId && f.OwnerId == account.Id, cancellationToken);
        
        if (file is null)
            return Result.Failure<string>(FileMetadataErrors.NotFound);
        
        Result<string> preSignedUrlResult = await storageServices.GetDownloadUrlAsync(file.FileKey, cancellationToken);
        
        if (preSignedUrlResult.IsFailure)
            return Result.Failure<string>(preSignedUrlResult.Error);
        
        return Result.Success(preSignedUrlResult.Value);
    }
}