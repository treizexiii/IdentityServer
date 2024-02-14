using Identity.Services.Admin;
using Identity.Services.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Services;

public static class IdentityServicesProvider
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthOptions, AuthOptions>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminService, AdminService>();
        return services;
    }
}