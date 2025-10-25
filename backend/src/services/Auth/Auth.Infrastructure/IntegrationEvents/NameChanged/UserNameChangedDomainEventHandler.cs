using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Auth.Infrastructure.IntegrationEvents.NameChanged;

internal sealed class UserNameChangedDomainEventHandler(
    ILogger<UserNameChangedDomainEventHandler> logger,
    IPublishEndpoint publishEndpoint) : INotificationHandler<DomainEventNotification<UserNameChangeDomainEvent>>
{
    public async Task Handle(DomainEventNotification<UserNameChangeDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.Event;
        
        var contract = new UserNameChangedContract
        (
            domainEvent.UserId,
            domainEvent.NewName,
            domainEvent.ChangedAt
        );
        
        logger.LogInformation("Publishing {Contract} for User {UserId} with new name {NewName}",
            nameof(UserNameChangedContract), domainEvent.UserId, domainEvent.NewName);
        
        await publishEndpoint.Publish(contract, cancellationToken);
        
        logger.LogInformation("Published {Contract} for User {UserId} with new name {NewName}",
            nameof(UserNameChangedContract), domainEvent.UserId, domainEvent.NewName);
    }
}