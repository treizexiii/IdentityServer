// See https://aka.ms/new-console-template for more information

using Identity.Core;
using Identity.Persistence;
using Identity.Persistence.Database;
using Identity.Services;
using Identity.Services.Admin;
using Identity.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tools.TransactionsManager;

namespace Identity.Migrator;

internal class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, Migrator!");
        Console.WriteLine("Loading environment variables...");
        DotEnv.Load(".env");

        Console.WriteLine("Building host...");
        var host = BuildHost();

        Console.WriteLine("Executing command...");
        await Execute(args, host);
    }

    private static IHost BuildHost()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(builder =>
            {
                builder.AddJsonFile("appsettings.json", true);
                builder.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                    true);
                builder.AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                var connectionString = IdentityPersistenceProvider.BuildConnectionString(
                    hostContext.Configuration["DB_HOST"],
                    hostContext.Configuration["DB_PORT"],
                    hostContext.Configuration["DB_NAME"],
                    hostContext.Configuration["DB_USER"],
                    hostContext.Configuration["DB_PASSWORD"]);
                Console.WriteLine(connectionString);
                services.AddPersistence(connectionString);
                services.AddTransactionManager<IdentityDb>();
                services.AddIdentityDomain();
                services.AddIdentityServices();
            })
            .Build();

        return host;
    }

    private static async Task Execute(string[] args, IHost host)
    {
        var command = args[0];
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Environment: {Env}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        logger.LogInformation("Databases host: {Host}", Environment.GetEnvironmentVariable("DB_HOST"));
        logger.LogInformation("Databases port: {Port}", Environment.GetEnvironmentVariable("DB_PORT"));
        logger.LogInformation("Databases name: {Name}", Environment.GetEnvironmentVariable("DB_NAME"));

        switch (command)
        {
            case "migrate":
                Migrate(host.Services.GetRequiredService<IdentityDb>());
                break;
            case "create":
                Create(host.Services.GetRequiredService<IdentityDb>());
                break;
            case "seed":
                await Seed(host.Services);
                break;
            case "drop":
                logger.LogWarning("Drop database not implemented");
                break;
            default:
                throw new Exception("Invalid command");
        }
    }

    private static void Migrate(DbContext context)
    {
        try
        {
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
                Console.WriteLine("Migrations applied");
            }
            else
            {
                Console.WriteLine("No pending migrations");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private static void Create(DbContext context)
    {
        try
        {
            context.Database.EnsureCreated();
            Console.WriteLine("Database created");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private static async Task Seed(IServiceProvider provider)
    {
        try
        {
            var transactionManager = provider.GetRequiredService<ITransactionManager>();
            var logger = provider.GetRequiredService<ILogger<SuperAdminBuilder>>();
            var adminService = provider.GetRequiredService<IAdminService>();
            var authService = provider.GetRequiredService<IAuthService>();
            var builder = new SuperAdminBuilder(transactionManager, logger, adminService, authService);
            await builder.Create();

            Console.WriteLine("Database seeded");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}