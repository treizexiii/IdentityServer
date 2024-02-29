using System.Text;
using Identity.Core.Entities;
using Identity.Core.Factories;
using Identity.Core.Managers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Core;

public static class IdentityCoreProvider
{
    public static IServiceCollection AddIdentityDomain(this IServiceCollection services)
    {
        services.AddScoped<ITokenManager, TokenManager>();
        services.AddScoped<ISecretManager, SecretManager>();
        services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory>();

        return services;
    }
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, string jwtKey)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),
        };
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
            });

        services.AddAuthorization();

        return services;
    }
}