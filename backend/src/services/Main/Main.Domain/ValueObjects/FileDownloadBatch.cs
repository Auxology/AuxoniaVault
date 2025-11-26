using Main.SharedKernel;

namespace Main.Domain.ValueObjects;

public readonly record struct FileDownloadBatch
{
    private const int MaxBatchSize = 50;

    private static Error FileIdsRequired => Error.Validation
    (
        "FileDownloadBatches.FileIdsRequired",
        "At least one file ID must be provided."
    );


    private static readonly Error NoFileIds = Error.Validation
    (
        "FileDownload.NoFileIds",
        "At least one file ID must be provided."
    );
    
    private static readonly Error ExceedsMaxBatchSize = Error.Validation
    (
        "FileDownload.ExceedsMaxBatchSize",
        $"The maximum number of file IDs per batch is {MaxBatchSize}."
    );
    
    public IReadOnlyList<FileMetadataId> FileIds { get; }
    
    private FileDownloadBatch(IReadOnlyList<FileMetadataId> fileIds)
    {
        FileIds = fileIds;
    }

    public static Result<FileDownloadBatch> Create(IEnumerable<Guid> fileIds)
    {
        if (fileIds is null)
            return Result.Failure<FileDownloadBatch>(FileIdsRequired);
        
        List<Guid> distinctFileIds = fileIds.Distinct().ToList();
        
        if (distinctFileIds.Count == 0)
            return Result.Failure<FileDownloadBatch>(NoFileIds);
        
        if (distinctFileIds.Count > MaxBatchSize)
            return Result.Failure<FileDownloadBatch>(ExceedsMaxBatchSize);
        
        IReadOnlyList<FileMetadataId> fileMetadataIds = distinctFileIds
            .Select(FileMetadataId.UnsafeFromGuid)
            .ToList();
        
        FileDownloadBatch batch = new(fileMetadataIds);
        
        return Result.Success(batch);
    }
}
