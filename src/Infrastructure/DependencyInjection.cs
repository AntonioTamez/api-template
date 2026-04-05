using Company.Template.Application.Abstractions.Messaging;
using Company.Template.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Template.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

        return services;
    }
}
