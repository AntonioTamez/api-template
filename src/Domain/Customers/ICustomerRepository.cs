using Company.Template.Domain.Abstractions;

namespace Company.Template.Domain.Customers;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}
