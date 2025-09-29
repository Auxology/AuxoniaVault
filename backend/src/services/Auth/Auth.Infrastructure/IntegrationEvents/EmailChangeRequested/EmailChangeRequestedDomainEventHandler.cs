using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Auth.Infrastructure.IntegrationEvents.EmailChangeRequested;

internal sealed class EmailChangeRequestedDomainEventHandler(IPublishEndpoint publishEndpoint, ILogger<EmailChangeRequestedDomainEventHandler> logger) : INotificationHandler<DomainEventNotification<EmailChangeRequestedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<EmailChangeRequestedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.Event;
        
        var contract = new EmailChangeRequestedContract
        (
            domainEvent.CurrentEmail,
            domainEvent.CurrentOtp,
            domainEvent.IpAddress,
            domainEvent.UserAgent,
            domainEvent.RequestedAt
        );

        logger.LogInformation("Publishing {Contract} for {Email}", nameof(EmailChangeRequestedContract), domainEvent.CurrentEmail);
        
        await publishEndpoint.Publish(contract, cancellationToken);
    }
}