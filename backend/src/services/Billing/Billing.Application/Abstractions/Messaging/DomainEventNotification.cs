using Billing.SharedKernel;
using MediatR;

namespace Billing.Application.Abstractions.Messaging;

public class DomainEventNotification<TEvent> : INotification
    where TEvent : IDomainEvent
{
    public TEvent Event { get; }

    public DomainEventNotification(TEvent @event) => Event = @event;
}