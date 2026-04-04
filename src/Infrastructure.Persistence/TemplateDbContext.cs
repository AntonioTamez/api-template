using Company.Template.Application.Abstractions.Data;
using Company.Template.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace Company.Template.Infrastructure.Persistence;

public sealed class TemplateDbContext : DbContext, IUnitOfWork
{
    public TemplateDbContext(DbContextOptions<TemplateDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TemplateDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
