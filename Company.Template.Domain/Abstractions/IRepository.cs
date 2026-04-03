namespace Company.Template.Domain.Abstractions;

public interface IRepository<TAggregate>
    where TAggregate : class, IAggregateRoot
{
    Task<TAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(TAggregate entity, CancellationToken cancellationToken = default);
}
