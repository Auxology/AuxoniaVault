using Main.Application.Abstractions.Authentication;
using Main.Application.Abstractions.Database;
using Main.Application.Abstractions.Messaging;
using Main.Application.Abstractions.Storage;
using Main.Application.Errors;
using Main.Domain.Aggregates.Account;
using Main.Domain.Aggregates.FailedCleanup;
using Main.Domain.Aggregates.FileMetadata;
using Main.Domain.ValueObjects;
using Main.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Main.Application.Files.DeleteFile;

internal sealed class DeleteFileCommandHandler(
    IMainDbContext context,
    IUserContext userContext,
    IStorageServices storageServices,
    IDateTimeProvider dateTimeProvider
)
    : ICommandHandler<DeleteFileCommand>
{
    public async Task<Result> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        UserId userId = UserId.UnsafeFromGuid(userContext.UserId);
        
        Account? account = await context.Accounts
            .FirstOrDefaultAsync(a => a.Id == userId, cancellationToken);

        if (account is null)
            return Result.Failure(AccountErrors.NotFound);
        
        FileMetadataId fileId = FileMetadataId.UnsafeFromGuid(request.FileId);

        FileMetadata? file = await context.Files
            .FirstOrDefaultAsync(f => f.Id == fileId && f.OwnerId == account.Id, cancellationToken);
        
        if (file is null)
            return Result.Failure(FileMetadataErrors.NotFound);
        
        context.Files.Remove(file);
        
        await context.SaveChangesAsync(cancellationToken);
        
        Result removeResult = await storageServices.RemoveFileAsync(file.FileKey, cancellationToken);

        if (removeResult.IsSuccess)
            return Result.Success();
        
        var failedCleanup = FailedCleanup.Create
        (
            fileId: file.Id,
            ownerId: account.Id,
            fileKey: file.FileKey,
            dateTimeProvider: dateTimeProvider
        );
        
        await context.FailedCleanups.AddAsync(failedCleanup, cancellationToken);
        
        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}