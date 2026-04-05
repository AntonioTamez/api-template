using Company.Template.Application.Abstractions.Messaging;
using Company.Template.Domain.Customers.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Company.Template.Application.Customers.Events;

internal sealed class CustomerRegisteredDomainEventHandler
    : INotificationHandler<DomainEventNotification<CustomerRegisteredDomainEvent>>
{
    private readonly ILogger<CustomerRegisteredDomainEventHandler> _logger;

    public CustomerRegisteredDomainEventHandler(ILogger<CustomerRegisteredDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<CustomerRegisteredDomainEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "Customer registered: {CustomerId} with email {Email} at {OccurredOnUtc}",
            domainEvent.CustomerId,
            domainEvent.Email,
            domainEvent.OccurredOnUtc);

        return Task.CompletedTask;
    }
}
