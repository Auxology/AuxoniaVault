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

internal sealed class FileMetadataUpdatedEventHandler
(
    IMainDbContext context,
    IOpenSearchService service,
    ILogger<FileMetadataUpdatedEventHandler> logger
)
    : INotificationHandler<DomainEventNotification<FileMetadataUpdatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<FileMetadataUpdatedDomainEvent> notification, 
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.Event;

        logger.LogInformation("Handling FileMetadataUpdatedDomainEvent for file {FileId}", domainEvent.FileId);

        FileMetadataId fileMetadataId = FileMetadataId.UnsafeFromGuid(domainEvent.FileId);
        
        FileMetadata? file = await context.Files
            .FirstOrDefaultAsync(fm => fm.Id == fileMetadataId, cancellationToken);

        if (file is null)
        {
            logger.LogWarning("FileMetadata with ID {FileId} not found in database", domainEvent.FileId);
            return;
        }
        
        var result = await service.UpdateFileMetadataAsync(file, cancellationToken);

        if (result.IsFailure)
            logger.LogError("Failed to update FileMetadata with ID {FileId} in OpenSearch: {Error}", domainEvent.FileId,
                result.Error);

        logger.LogInformation("Successfully updated FileMetadata with ID {FileId} in OpenSearch", domainEvent.FileId);
    }
}