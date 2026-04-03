using System.Threading.RateLimiting;
using Company.Template.Api.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Company.Template.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.SectionName));
        var rateLimitingOptions = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "global",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = rateLimitingOptions.PermitLimit,
                    Window = TimeSpan.FromSeconds(rateLimitingOptions.WindowInSeconds),
                    QueueLimit = rateLimitingOptions.QueueLimit,
                    QueueProcessingOrder = Enum.TryParse<QueueProcessingOrder>(rateLimitingOptions.QueueProcessingOrder, out var order)
                        ? order
                        : QueueProcessingOrder.OldestFirst
                }));
        });

        return services;
    }
}
