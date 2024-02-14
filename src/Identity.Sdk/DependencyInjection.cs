using Microsoft.Extensions.DependencyInjection;

namespace Identity.Sdk;

public static class DependencyInjection
{
    public static IServiceCollection AddRestClient(IServiceCollection services, string url)
    {
        services.AddHttpClient<IIdentityClient, IdentityHttpClient>(client =>
        {
            client.BaseAddress = new Uri(url);
        });

        return services;
    }
}