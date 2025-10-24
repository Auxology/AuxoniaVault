using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Events;
using Auth.SharedKernel;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Auth.Infrastructure.IntegrationEvents.SignUp;

internal sealed class UserCreatedDomainEventHandler(
    ILogger<UserCreatedDomainEventHandler> logger,
    IPublishEndpoint publishEndpoint) : INotificationHandler<DomainEventNotification<UserCreatedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<UserCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.Event;

        var contract = new UserCreatedContract
        (
            domainEvent.UserId,
            domainEvent.Email,
            domainEvent.Name,
            domainEvent.CreatedAt
        );

        logger.LogInformation("Publishing {Contract} for User {UserId} with email {Email}",
            nameof(UserCreatedContract), domainEvent.UserId, domainEvent.Email);
        
        await publishEndpoint.Publish(contract, cancellationToken);
        
        logger.LogInformation("Published {Contract} for User {UserId} with email {Email}",
            nameof(UserCreatedContract), domainEvent.UserId, domainEvent.Email);
    }
}