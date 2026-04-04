using Company.Template.Application.Abstractions.Data;
using Company.Template.Domain.Customers;
using Company.Template.Infrastructure.Persistence.Repositories;
using Company.Template.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Company.Template.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PersistenceOptions>(configuration.GetSection(PersistenceOptions.SectionName));

        services.AddDbContext<TemplateDbContext>((provider, options) =>
        {
            var persistenceOptions = provider.GetRequiredService<IOptions<PersistenceOptions>>().Value;
            options.UseNpgsql(persistenceOptions.ConnectionString, b => b.MigrationsAssembly(typeof(TemplateDbContext).Assembly.FullName));
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TemplateDbContext>());
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<TemplateDbContextSeeder>();

        return services;
    }
}
