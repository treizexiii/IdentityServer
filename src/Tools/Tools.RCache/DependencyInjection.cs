using Microsoft.Extensions.DependencyInjection;

namespace Tools.RCache;

public static class DependencyInjection
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, string connectionString)
    {
        services.AddStackExchangeRedisCache(setup =>
        {
            setup.Configuration = connectionString;

        });

        return services;
    }
}