using Billing.Application.Abstractions.Messaging;
using Billing.Domain.Events;
using Billing.Infrastructure.Services;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Contracts;

namespace Billing.Infrastructure.IntegrationEvents.SubscriptionCanceled;

internal sealed class SubscriptionCanceledDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    ILogger<SubscriptionCanceledDomainEventHandler> logger) : INotificationHandler<DomainEventNotification<SubscriptionCanceledDomainEvent>>
{
    public async Task Handle(DomainEventNotification<SubscriptionCanceledDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.Event;

        var contract = new SubscriptionCanceledContract
        (
            UserId: domainEvent.UserId,
            StripeCustomerName: domainEvent.StripeCustomerName,
            StripeCustomerEmail: domainEvent.StripeCustomerEmail,
            Tier: 0,
            PriceFormatted: domainEvent.PriceFormatted,
            PlanName: domainEvent.ProductName,
            CanceledAt: domainEvent.CanceledAt
        );
        
        logger.LogInformation(
            "Publishing SubscriptionCanceledContract for UserId: {UserId}, StripeSubscriptionId: {StripeSubscriptionId}",
            domainEvent.UserId, domainEvent.StripeSubscriptionId);
        
        await publishEndpoint.Publish(contract, cancellationToken);
        
        logger.LogInformation(
            "Published SubscriptionCanceledContract for UserId: {UserId}, StripeSubscriptionId: {StripeSubscriptionId}",
            domainEvent.UserId, domainEvent.StripeSubscriptionId);
    }
}