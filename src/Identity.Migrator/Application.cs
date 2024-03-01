using Identity.Persistence.Database;
using Identity.Services.Admin;
using Identity.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tools.TransactionsManager;

namespace Identity.Migrator;

public class Application
{
    private readonly ILogger<Application> _logger;
    private readonly IHost _host;

    public Application(IHost host)
    {
        _host = host;
        _logger = _host.Services.GetRequiredService<ILogger<Application>>();
    }

    public async Task RunAsync(string[] args)
    {
        _logger.LogInformation("Environment: {Env}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        _logger.LogInformation("Databases host: {Host}", Environment.GetEnvironmentVariable("DB_HOST"));
        _logger.LogInformation("Databases port: {Port}", Environment.GetEnvironmentVariable("DB_PORT"));
        _logger.LogInformation("Databases name: {Name}", Environment.GetEnvironmentVariable("DB_NAME"));

        while (true)
        {
            DisplayMenu();
            int choice;
            do
            {
                Console.Write("Enter your choice: \n");

                if (args.Length > 0)
                {
                    choice = int.TryParse(args[0], out choice) ? choice : -1;
                    args = ["0"];
                }
                else
                {
                    var key = Console.ReadLine();
                    choice = int.TryParse(key, out choice) ? choice : -1;
                }

                if (choice is < 0 or > 3)
                {
                    _logger.LogInformation("Invalid command");
                }
            } while (choice is < 0 or > 3);

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
            var context = _host.Services.GetRequiredService<IdentityDb>();
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
                _logger.LogInformation("Migrations applied");
            }
            else
            {
                _logger.LogInformation("No pending migrations");
            }
        }
        catch (Exception e)
        {
            _logger.LogInformation("{EMessage}", e.Message);
        }
    }

    private void Create()
    {
        try
        {
            var context = _host.Services.GetRequiredService<IdentityDb>();
            context.Database.EnsureCreated();
            _logger.LogInformation("Database created");
        }
        catch (Exception e)
        {
            _logger.LogInformation("{EMessage}", e.Message);
        }
    }

    private async Task Seed()
    {
        try
        {
            var provider = _host.Services.CreateScope().ServiceProvider;
            var transactionManager = provider.GetRequiredService<ITransactionManager>();
            var logger = provider.GetRequiredService<ILogger<SuperAdminBuilder>>();
            var adminService = provider.GetRequiredService<IAdminService>();
            var authService = provider.GetRequiredService<IAuthService>();
            var builder = new SuperAdminBuilder(transactionManager, logger, adminService, authService);
            await builder.Create();

            _logger.LogInformation("Database seeded");
        }
        catch (Exception e)
        {
            _logger.LogInformation("{EMessage}", e.Message);
        }
    }
}