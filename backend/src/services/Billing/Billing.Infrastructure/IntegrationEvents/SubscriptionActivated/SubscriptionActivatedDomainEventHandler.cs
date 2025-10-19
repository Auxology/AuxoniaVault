using Billing.Application.Abstractions.Messaging;
using Billing.Domain.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Billing.Infrastructure.IntegrationEvents.SubscriptionActivated;

internal sealed class SubscriptionActivatedDomainEventHandler(IPublishEndpoint publishEndpoint, ILogger<SubscriptionActivatedDomainEventHandler> logger) : INotificationHandler<DomainEventNotification<SubscriptionActivatedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<SubscriptionActivatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.Event;

        var contract = new SubscriptionActivatedContract
        (
            domainEvent.UserId,
            domainEvent.StripeCustomerName,
            domainEvent.StripeCustomerEmail,
            domainEvent.PriceFormatted,
            domainEvent.ProductName,
            domainEvent.CurrentPeriodStart,
            domainEvent.CurrentPeriodEnd
        );

        logger.LogInformation(
            "Publishing SubscriptionActivatedContract for UserId: {UserId}, StripeSubscriptionId: {StripeSubscriptionId}",
            domainEvent.UserId, domainEvent.StripeSubscriptionId);
        
        await publishEndpoint.Publish(contract, cancellationToken);

        logger.LogInformation(
            "Published SubscriptionActivatedContract for UserId: {UserId}, StripeSubscriptionId: {StripeSubscriptionId}",
            domainEvent.UserId, domainEvent.StripeSubscriptionId);
    }
}