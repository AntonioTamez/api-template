using Company.Template.Domain.Abstractions;

namespace Company.Template.Domain.Customers.Events;

public sealed record CustomerRegisteredDomainEvent(Guid EventId, CustomerId CustomerId, string Email) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
