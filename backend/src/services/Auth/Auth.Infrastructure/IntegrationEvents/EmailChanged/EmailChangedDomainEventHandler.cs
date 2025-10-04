using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Auth.Infrastructure.IntegrationEvents.EmailChanged;

internal sealed class EmailChangedDomainEventHandler(IPublishEndpoint publishEndpoint, ILogger<EmailChangedDomainEventHandler> logger) : INotificationHandler<DomainEventNotification<EmailChangedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<EmailChangedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.Event;

        var contract = new EmailChangedContract
        (
            domainEvent.UserId,
            domainEvent.NewEmail,
            domainEvent.ChangedAt
        );

        logger.LogInformation("Publishing {Contract} for User {UserId} with new email {NewEmail}",
            nameof(EmailChangedContract), domainEvent.UserId, domainEvent.NewEmail);

        await publishEndpoint.Publish(contract, cancellationToken);

        logger.LogInformation("Published {Contract} for User {UserId} with new email {NewEmail}",
            nameof(EmailChangedContract), domainEvent.UserId, domainEvent.NewEmail);
    }
}
