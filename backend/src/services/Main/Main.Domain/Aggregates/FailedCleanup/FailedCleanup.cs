using Main.Domain.Errors;
using Main.Domain.ValueObjects;
using Main.SharedKernel;

namespace Main.Domain.Aggregates.FailedCleanup;

public sealed class FailedCleanup : Entity, IAggregateRoot
{
    public int Id { get; private set; }
    
    public FileMetadataId FileId { get; private set; }
    
    public UserId OwnerId { get; private set; }
    
    public string FileKey { get; private set; }
    
    public DateTimeOffset FailedAt { get; private set; }
    
    private FailedCleanup() { } // For EF Core

    private FailedCleanup
    (
        FileMetadataId fileId,
        UserId ownerId,
        string fileKey,
        DateTimeOffset utcNow
    )
    {
        FileId = fileId;
        OwnerId = ownerId;
        FileKey = fileKey;
        FailedAt = utcNow;
    }

    public static FailedCleanup Create(FileMetadataId fileId, UserId ownerId, string fileKey,
        IDateTimeProvider dateTimeProvider)
    {
        FailedCleanup failedCleanup = new
        (
            fileId,
            ownerId,
            fileKey,
            dateTimeProvider.UtcNow
        );

        return failedCleanup;
    }
}