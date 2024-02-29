using Identity.Core.Configuration;
using Identity.Services.Admin;
using Identity.Services.Auth;
using Identity.Services.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Services;

public static class IdentityServicesProvider
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddScoped<IJwtOptions, JwtOptions>();
        services.AddScoped<ISecurityProvider, SecurityProvider>();
        services.AddScoped<IAuthOptions, AuthOptions>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminService, AdminService>();
        return services;
    }
}