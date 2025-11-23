using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Auth.Infrastructure.IntegrationEvents.UserRecovered;

internal sealed class UserRecoveredDomainEventHandler(IPublishEndpoint publishEndpoint, ILogger<UserRecoveredDomainEventHandler> logger) : INotificationHandler<DomainEventNotification<UserRecoveredDomainEvent>>
{
    public async Task Handle(DomainEventNotification<UserRecoveredDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.Event;

        UserRecoveredContract contract = new
        (
            UserId: domainEvent.UserId,
            NewEmail: domainEvent.NewEmail,
            IpAddress: domainEvent.IpAddress,
            UserAgent: domainEvent.UserAgent,
            RecoveredAt: domainEvent.RecoveredAt
        );
        
        logger.LogInformation("Publishing {Contract} for User {UserId} with email {NewEmail}",
            nameof(UserRecoveredContract), domainEvent.UserId, domainEvent.NewEmail);
        
        await publishEndpoint.Publish(contract, cancellationToken);
        
        logger.LogInformation("Published {Contract} for User {UserId} with email {NewEmail}",
            nameof(UserRecoveredContract), domainEvent.UserId, domainEvent.NewEmail);
    }
}