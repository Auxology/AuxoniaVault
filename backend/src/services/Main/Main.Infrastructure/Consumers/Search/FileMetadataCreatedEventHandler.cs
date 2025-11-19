using Main.Application.Abstractions.Database;
using Main.Application.Abstractions.Messaging;
using Main.Domain.Aggregates.FileMetadata;
using Main.Domain.Events;
using Main.Domain.ValueObjects;
using Main.Infrastructure.Search.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Main.Infrastructure.Consumers.Search;

internal sealed class FileMetadataCreatedEventHandler(
    IMainDbContext context,
    IOpenSearchService service,
    ILogger<FileMetadataCreatedEventHandler> logger
)
    : INotificationHandler<DomainEventNotification<FileMetadataCreatedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<FileMetadataCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.Event;
        
        logger.LogInformation("Handling FileMetadataCreatedDomainEvent for FileId: {FileId}", domainEvent.FileId);
        
        FileMetadataId fileMetadataId = FileMetadataId.UnsafeFromGuid(domainEvent.FileId);

        FileMetadata? file = await context.Files
            .FirstOrDefaultAsync(f => f.Id == fileMetadataId, cancellationToken);

        if (file is null)
        {
            logger.LogWarning("FileMetadata with Id: {FileId} not found in database.", domainEvent.FileId);
            return;
        }
        
        var result = await service.IndexFileMetadataAsync(file, cancellationToken);

        if (result.IsFailure)
            logger.LogError("Failed to index FileMetadata with Id: {FileId}. Error: {Error}", domainEvent.FileId,
                result.Error);
        
        logger.LogInformation("Successfully indexed FileMetadata with Id: {FileId}", domainEvent.FileId);
    }
}