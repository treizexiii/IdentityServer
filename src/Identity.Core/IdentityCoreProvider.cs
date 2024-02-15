using Identity.Core.Entities;
using Identity.Core.Factories;
using Identity.Core.Managers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Core;

public static class IdentityCoreProvider
{
    public static IServiceCollection AddIdentityDomain(this IServiceCollection services)
    {
        // services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User, Role>>();
        services.AddScoped<ITokenManager, TokenManager>();
        services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory>();

        return services;
    }
}