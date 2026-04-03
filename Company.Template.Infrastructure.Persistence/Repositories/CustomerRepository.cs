using Company.Template.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace Company.Template.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository : ICustomerRepository
{
    private readonly TemplateDbContext _dbContext;

    public CustomerRepository(TemplateDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Customers.AddAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers.AnyAsync(customer => customer.Email.Value == email, cancellationToken);
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers.AsNoTracking().FirstOrDefaultAsync(customer => customer.Id.Value == id, cancellationToken);
    }
}
