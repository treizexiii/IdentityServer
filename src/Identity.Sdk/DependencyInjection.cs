using Microsoft.Extensions.DependencyInjection;

namespace Identity.Sdk;

public static class DependencyInjection
{
    public static IServiceCollection AddRestClient(IServiceCollection services, string url)
    {
        services.AddHttpClient<IIdentityClient, IdentityHttpClient>(client =>
        {
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Add("Access-Control-Allow-Origin", "*");
        });

        return services;
    }
}