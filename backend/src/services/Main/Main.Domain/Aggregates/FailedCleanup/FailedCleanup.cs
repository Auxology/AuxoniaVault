using Main.Domain.Errors;
using Main.Domain.ValueObjects;
using Main.SharedKernel;

namespace Main.Domain.Aggregates.FailedCleanup;

public sealed class FailedCleanup : Entity, IAggregateRoot
{
    public int Id { get; private set; }
    
    public FileMetadataId FileId { get; private set; }
    
    public UserId UserId { get; private set; }
    
    public string FileKey { get; private set; }
    
    public DateTimeOffset FailedAt { get; private set; }
    
    private FailedCleanup() { } // For EF Core

    private FailedCleanup
    (
        FileMetadataId fileId,
        UserId userId,
        string fileKey,
        DateTimeOffset utcNow
    )
    {
        FileId = fileId;
        UserId = userId;
        FileKey = fileKey;
        FailedAt = utcNow;
    }

    public static FailedCleanup Create(FileMetadataId fileId, UserId userId, string fileKey,
        IDateTimeProvider dateTimeProvider)
    {
        FailedCleanup failedCleanup = new
        (
            fileId,
            userId,
            fileKey,
            dateTimeProvider.UtcNow
        );

        return failedCleanup;
    }
}