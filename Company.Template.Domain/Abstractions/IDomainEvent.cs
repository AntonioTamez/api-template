namespace Company.Template.Domain.Abstractions;

public interface IDomainEvent
{
    Guid EventId { get; }

    DateTime OccurredOnUtc { get; }
}
