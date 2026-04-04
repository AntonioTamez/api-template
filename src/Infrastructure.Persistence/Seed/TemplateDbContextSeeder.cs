using Company.Template.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Company.Template.Infrastructure.Persistence.Seed;

public sealed class TemplateDbContextSeeder
{
    private readonly TemplateDbContext _dbContext;
    private readonly ILogger<TemplateDbContextSeeder> _logger;

    public TemplateDbContextSeeder(TemplateDbContext dbContext, ILogger<TemplateDbContextSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

        if (await _dbContext.Customers.AnyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        _logger.LogInformation("Seeding initial customers...");

        var customers = new[]
        {
            CreateCustomer("Ada", "Lovelace", "ada@example.com"),
            CreateCustomer("Alan", "Turing", "alan@example.com"),
            CreateCustomer("Grace", "Hopper", "grace@example.com")
        };

        await _dbContext.Customers.AddRangeAsync(customers, cancellationToken).ConfigureAwait(false);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Seeded {Count} customers", customers.Length);
    }

    private static Customer CreateCustomer(string firstName, string lastName, string email)
    {
        var result = Customer.Create(firstName, lastName, email);

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Unable to seed customer '{email}': {result.Error.Message}");
        }

        return result.Value;
    }
}
