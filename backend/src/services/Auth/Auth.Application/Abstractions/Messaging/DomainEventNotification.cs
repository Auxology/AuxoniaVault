using Auth.SharedKernel;
using MediatR;

namespace Auth.Application.Abstractions.Messaging;

public class DomainEventNotification<TEvent> : INotification
    where TEvent : IDomainEvent
{
    public TEvent Event { get; }

    public DomainEventNotification(TEvent @event) => Event = @event;
}