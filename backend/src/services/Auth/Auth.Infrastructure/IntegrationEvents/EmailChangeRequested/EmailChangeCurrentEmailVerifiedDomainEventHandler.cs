using Auth.Application.Abstractions.Messaging;
using Auth.Domain.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Auth.Infrastructure.IntegrationEvents.EmailChangeRequested;

internal sealed class EmailChangeCurrentEmailVerifiedDomainEventHandler(IPublishEndpoint publishEndpoint, ILogger<EmailChangeCurrentEmailVerifiedDomainEventHandler> logger)
    : INotificationHandler<DomainEventNotification<EmailChangeCurrentEmailVerifiedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<EmailChangeCurrentEmailVerifiedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.Event;

        var contract = new Shared.Contracts.EmailChangeCurrentEmailVerifiedContract(
            domainEvent.NewEmail,
            domainEvent.NewOtp,
            domainEvent.RequestedAt
        );

        logger.LogInformation("Publishing {Contract} for {Email}", nameof(EmailChangeCurrentEmailVerifiedContract), domainEvent.NewEmail);

        await publishEndpoint.Publish(contract, cancellationToken);

        logger.LogInformation("Published {Contract} for {Email}", nameof(EmailChangeCurrentEmailVerifiedContract), domainEvent.NewEmail);
    }
}