using Identity.Core.Entities;
using Identity.Core.Repositories;
using Identity.Persistence.Database;
using Identity.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Identity.Persistence;

public static class IdentityPersistenceProvider
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<IdentityDb>(options =>
        {
            options.UseNpgsql(connectionString);
            options.UseLowerCaseNamingConvention();
            options.UseLazyLoadingProxies();
        });

        services.AddRepositories();

        return services;
    }

    public static string BuildConnectionString(string host, string port, string database, string username,
        string password)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder();
        connectionStringBuilder.Host = host;
        connectionStringBuilder.Port = int.Parse(port);
        connectionStringBuilder.Database = database;
        connectionStringBuilder.Username = username;
        connectionStringBuilder.Password = password;

        return connectionStringBuilder.ToString();
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<IUserStore<User>, UsersRepositories>();
        services.AddTransient<ITokenStore<UserToken>, TokenStore>();
        services.AddTransient<ISecretStore<Secret>, SecretStore>();
        services.AddTransient<IUsersRepository, UsersRepositories>();
        services.AddTransient<IRolesRepository, RolesRepository>();
        services.AddTransient<IAppsRepository, AppsRepository>();
        return services;
    }
}
