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

namespace Main.Application.Files.DownloadMultipleFiles;

internal sealed class DownloadMultipleFilesCommandHandler
(
    IMainDbContext context,
    IUserContext userContext,
    IStorageServices storageServices
) : ICommandHandler<DownloadMultipleFilesCommand, DownloadMultipleFilesResponse>
{
    public async Task<Result<DownloadMultipleFilesResponse>> Handle(DownloadMultipleFilesCommand request, CancellationToken cancellationToken)
    {
        Result<FileDownloadBatch> batchResult = FileDownloadBatch.Create(request.FileIds);
        
        if (batchResult.IsFailure)
            return Result.Failure<DownloadMultipleFilesResponse>(batchResult.Error);
        
        FileDownloadBatch batch = batchResult.Value;
        
        UserId userId = UserId.UnsafeFromGuid(userContext.UserId); 
        
        Account? account = await context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == userId, cancellationToken);
        
        if (account is null)
            return Result.Failure<DownloadMultipleFilesResponse>(AccountErrors.NotFound);
        
        List<FileMetadata> files = await context.Files
            .Where(f => batch.FileIds.Contains(f.Id) && f.OwnerId == account.Id)
            .ToListAsync(cancellationToken);

        if (files.Count != batch.FileIds.Count)
            return Result.Failure<DownloadMultipleFilesResponse>(FileMetadataErrors.SomeFilesNotFound);
            
        
        Dictionary<Guid, FileMetadata> fileDict = files.ToDictionary(f => f.Id.Value);
        
        List<Task<FileDownloadResult>> downloadTasks = batch.FileIds
            .Select(fileId => ProcessFileDownloadAsync
            (
                fileId, 
                fileDict, 
                storageServices, 
                cancellationToken
            ))
            .ToList();
        
        FileDownloadResult[] downloadResults = await Task.WhenAll(downloadTasks);
        
        int successCount = downloadResults.Count(r => r.IsSuccess);
        int failureCount = downloadResults.Length - successCount;
        
        
        DownloadMultipleFilesResponse response = new
        (
            Results: downloadResults.ToList(),
            SuccessCount: successCount,
            FailureCount: failureCount
        );
        return Result.Success(response);
    }

    private static async Task<FileDownloadResult> ProcessFileDownloadAsync
    (
        FileMetadataId fileId,
        Dictionary<Guid, FileMetadata> fileDict,
        IStorageServices storageServices,
        CancellationToken cancellationToken
    )
    {
        Result<string> result = await storageServices.GetDownloadUrlAsync(fileId.ToString(), cancellationToken);

        FileMetadata file = fileDict[fileId.Value];
        
        if (result.IsFailure)
        {
            return new FileDownloadResult
            (
                FileId: fileId.Value,
                FileName: file.FileName,
                IsSuccess: false,
                ErrorMessage: result.Error.Description,
                DownloadUrl: null
            );
        }
        
        return new FileDownloadResult
        (
            FileId: fileId.Value,
            FileName: file.FileName,
            IsSuccess: true,
            ErrorMessage: null,
            DownloadUrl: result.Value
        );
    }
}