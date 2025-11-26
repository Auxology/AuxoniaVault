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
using Microsoft.Extensions.Logging;

namespace Main.Application.Files.DownloadMultipleFiles;

internal sealed class DownloadMultipleFilesCommandHandler
(
    IMainDbContext context,
    IUserContext userContext,
    IStorageServices storageServices,
    ILogger<DownloadMultipleFilesCommandHandler> logger
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
            .AsNoTracking()
            .Where(f => batch.FileIds.Contains(f.Id) && f.OwnerId == account.Id)
            .ToListAsync(cancellationToken);

        Dictionary<Guid, FileMetadata> fileDict = files.ToDictionary(f => f.Id.Value);
        Dictionary<string, Guid> keyToIdMap = files.ToDictionary(f => f.FileKey, f => f.Id.Value);

        List<string> fileKeys = files.Select(f => f.FileKey).ToList();

        Result<Dictionary<string, string>> urlsResult = await storageServices.GetDownloadUrlsAsync(fileKeys, cancellationToken);

        if (urlsResult.IsFailure)
            return Result.Failure<DownloadMultipleFilesResponse>(urlsResult.Error);

        Dictionary<string, string> urlMap = urlsResult.Value;

        List<FileDownloadResult> results = batch.FileIds
            .Select(fileId => BuildFileDownloadResult(fileId, fileDict, keyToIdMap, urlMap))
            .ToList();

        int successCount = results.Count(r => r.IsSuccess);
        int failureCount = results.Count - successCount;

        DownloadMultipleFilesResponse response = new
        (
            Results: results,
            SuccessCount: successCount,
            FailureCount: failureCount
        );

        logger.LogInformation(
            "User {UserId} downloaded {SuccessCount}/{TotalCount} files",
            userId.Value,
            successCount,
            results.Count);

        return Result.Success(response);
    }
    
    private static FileDownloadResult BuildFileDownloadResult
    (
        FileMetadataId fileId,
        Dictionary<Guid, FileMetadata> fileDict,
        Dictionary<string, Guid> keyToIdMap,
        Dictionary<string, string> urlMap
    )
    {
        if (!fileDict.TryGetValue(fileId.Value, out FileMetadata? file))
        {
            return new FileDownloadResult
            (
                FileId: fileId.Value,
                FileName: null,
                IsSuccess: false,
                ErrorMessage: "File not found or access denied",
                DownloadUrl: null
            );
        }

        if (!urlMap.TryGetValue(file.FileKey, out string? downloadUrl))
        {
            return new FileDownloadResult
            (
                FileId: fileId.Value,
                FileName: file.FileName,
                IsSuccess: false,
                ErrorMessage: "Failed to generate download URL",
                DownloadUrl: null
            );
        }

        return new FileDownloadResult
        (
            FileId: fileId.Value,
            FileName: file.FileName,
            IsSuccess: true,
            ErrorMessage: null,
            DownloadUrl: downloadUrl
        );
    }
}