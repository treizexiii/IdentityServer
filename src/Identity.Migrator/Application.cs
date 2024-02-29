using Identity.Persistence.Database;
using Identity.Services.Admin;
using Identity.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tools.TransactionsManager;

namespace Identity.Migrator;

public class Application(IHost host)
{
    public async Task RunAsync()
    {
        var logger = host.Services.GetRequiredService<ILogger<Application>>();

        logger.LogInformation("Environment: {Env}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        logger.LogInformation("Databases host: {Host}", Environment.GetEnvironmentVariable("DB_HOST"));
        logger.LogInformation("Databases port: {Port}", Environment.GetEnvironmentVariable("DB_PORT"));
        logger.LogInformation("Databases name: {Name}", Environment.GetEnvironmentVariable("DB_NAME"));

        while (true)
        {
            DisplayMenu();
            int choice;
            do
            {
                Console.Write("Enter your choice: ");
            } while (!int.TryParse(Console.ReadLine(), out choice) || choice < 0 || choice > 3);

            switch (choice)
            {
                case 2:
                    Migrate();
                    break;
                case 1:
                    Create();
                    break;
                case 3:
                    await Seed();
                    break;
                case 0:
                    return;
                default:
                    throw new Exception("Invalid command");
            }
        }
    }

    private static void DisplayMenu()
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine("1 - Create database");
        Console.WriteLine("2 - Migrate database");
        Console.WriteLine("3 - Seed database");
        Console.WriteLine("0 - Exit");
    }

    private void Migrate()
    {
        try
        {
            var context = host.Services.GetRequiredService<IdentityDb>();
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

    private void Create()
    {
        try
        {
            var context = host.Services.GetRequiredService<IdentityDb>();
            context.Database.EnsureCreated();
            Console.WriteLine("Database created");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private async Task Seed()
    {
        try
        {
            var provider = host.Services.CreateScope().ServiceProvider;
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