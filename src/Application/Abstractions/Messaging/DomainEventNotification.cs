using Company.Template.Domain.Abstractions;
using MediatR;

namespace Company.Template.Application.Abstractions.Messaging;

public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;
