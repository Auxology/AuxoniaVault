using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Auth.Infrastructure.IntegrationEvents.LoginRequested;

internal sealed class LoginRequestedDomainEventHandler(IPublishEndpoint publishEndpoint, ILogger<LoginRequestedDomainEventHandler> logger) : INotificationHandler<DomainEventNotification<LoginRequestedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<LoginRequestedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.Event;

        var contract = new LoginRequestedContract
        (
            domainEvent.Email,
            domainEvent.Token,
            domainEvent.RequestedAt
        );

        logger.LogInformation("Publishing {Contract} for {Email}", nameof(LoginRequestedContract), domainEvent.Email);

        await publishEndpoint.Publish(contract, cancellationToken);

        logger.LogInformation("Published {Contract} for {Email}", nameof(LoginRequestedContract), domainEvent.Email);
    }
}