using Main.Domain.Constants;
using Main.Domain.Errors;
using Main.Domain.Events;
using Main.Domain.ValueObjects;
using Main.SharedKernel;

namespace Main.Domain.Aggregates.FileMetadata;

public sealed class FileMetadata : Entity, IAggregateRoot
{
    public FileMetadataId Id { get; private set; }
    
    public UserId OwnerId { get; private set; }
    
    public string FileName { get; private set; }
    
    public string ContentType { get; private set; }
    
    public long FileSizeInBytes { get; private set; }
    
    public string FileKey { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset? ModifiedAt { get; private set; }
    
    public string? Description { get; private set; }
    
    public bool IsStarred { get; private set; }
    
    private FileMetadata() { }

    private FileMetadata
    (
        UserId ownerId,
        string fileName,
        string contentType,
        long fileSizeInBytes,
        string fileKey,
        DateTimeOffset createdAt
    )

    {
        Id = FileMetadataId.New();
        OwnerId = ownerId;
        FileName = fileName;
        ContentType = contentType;
        FileSizeInBytes = fileSizeInBytes;
        FileKey = fileKey;
        CreatedAt = createdAt;
        IsStarred = false;
    }
    
    public static Result<FileMetadata> Create(UserId ownerId, string fileName, string contentType, long fileSizeInBytes, string fileKey, IDateTimeProvider dateTimeProvider)
    {
        if (ownerId.IsEmpty())
            return Result.Failure<FileMetadata>(FileMetadataErrors.UserIdRequired);
        
        if (string.IsNullOrWhiteSpace(fileName))
            return Result.Failure<FileMetadata>(FileMetadataErrors.NameRequired);
        
        if (fileName.Length > FileConstants.MaxFileNameLength)
            return Result.Failure<FileMetadata>(FileMetadataErrors.FileNameTooLong);
        
        if (string.IsNullOrWhiteSpace(contentType))
            return Result.Failure<FileMetadata>(FileMetadataErrors.ContentTypeRequired);
        
        if (fileSizeInBytes < 0)
            return Result.Failure<FileMetadata>(FileMetadataErrors.InvalidFileSize);
        
        if (string.IsNullOrWhiteSpace(fileKey))
            return Result.Failure<FileMetadata>(FileMetadataErrors.FileKeyRequired);
        
        DateTimeOffset utcNow = dateTimeProvider.UtcNow;

        FileMetadata fileMetadata =
            new FileMetadata(ownerId, fileName, contentType, fileSizeInBytes, fileKey, utcNow);

        fileMetadata.Raise(new FileMetadataCreatedDomainEvent
        (
            FileId: fileMetadata.Id.Value,
            OwnerId: fileMetadata.OwnerId.Value,
            FileName: fileMetadata.FileName,
            ContentType: fileMetadata.ContentType,
            FileSizeInBytes: fileMetadata.FileSizeInBytes,
            FileKey: fileMetadata.FileKey,
            CreatedAt: fileMetadata.CreatedAt
        ));
        
        return Result.Success(fileMetadata);
    }

    public Result UpdateDescription(string description, IDateTimeProvider dateTimeProvider)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(FileMetadataErrors.UpdateDescriptionRequired);
        
        if (description.Length > FileConstants.MaxDescriptionLength)
            return Result.Failure(FileMetadataErrors.DescriptionTooLong);
        
        Description = description;

        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        ModifiedAt = utcNow;
        
        Raise(new FileMetadataUpdatedDomainEvent
        (
            FileId: Id.Value,
            OwnerId: OwnerId.Value,
            FileName: FileName,
            FileDescription: Description,
            IsStarred: IsStarred,
            ModifiedAt: utcNow
        ));
        
        return Result.Success();
    }

    public Result ToggleStar(IDateTimeProvider dateTimeProvider)
    {
        IsStarred = !IsStarred;

        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        ModifiedAt = utcNow;
        
        Raise(new FileMetadataUpdatedDomainEvent
        (
            FileId: Id.Value,
            OwnerId: OwnerId.Value,
            FileName: FileName,
            FileDescription: Description,
            IsStarred: IsStarred,
            ModifiedAt: utcNow
        ));
        
        return Result.Success();
    }
    
    public Result SetStarred(bool isStarred, IDateTimeProvider dateTimeProvider)
    {
        if (IsStarred == isStarred)
            return Result.Success();
        
        IsStarred = isStarred;

        DateTimeOffset utcNow = dateTimeProvider.UtcNow;
        
        ModifiedAt = utcNow;
        
        Raise(new FileMetadataUpdatedDomainEvent
        (
            FileId: Id.Value,
            OwnerId: OwnerId.Value,
            FileName: FileName,
            FileDescription: Description,
            IsStarred: IsStarred,
            ModifiedAt: utcNow
        ));
        
        return Result.Success();
    }
}