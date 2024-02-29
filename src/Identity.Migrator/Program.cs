// See https://aka.ms/new-console-template for more information

using Identity.Core;
using Identity.Migrator;
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

Console.WriteLine("Hello, Migrator!");

DotEnv.Load(".env");

// var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
// Console.WriteLine("Environment: " + env);
// Console.WriteLine("Databases host: " + Environment.GetEnvironmentVariable("DB_HOST"));
// Console.WriteLine("Databases port: " + Environment.GetEnvironmentVariable("DB_PORT"));
// Console.WriteLine("Databases name: " + Environment.GetEnvironmentVariable("DB_NAME"));

var host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(builder =>
    {
        builder.AddJsonFile("appsettings.json", true);
        builder.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true);
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
        await Seed(host.Services.GetRequiredService<IdentityDb>());
        break;
    case "drop":
        logger.LogWarning("Drop database not implemented");
        break;
    default:
        throw new Exception("Invalid command");
}

void Migrate(DbContext context)
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

void Create(DbContext context)
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

async Task Seed(DbContext context)
{
    try
    {
        var transactionManager = host.Services.GetRequiredService<ITransactionManager>();
        var logger = host.Services.GetRequiredService<ILogger<SuperAdminBuilder>>();
        var adminService = host.Services.GetRequiredService<IAdminService>();
        var authService = host.Services.GetRequiredService<IAuthService>();
        var builder = new SuperAdminBuilder(transactionManager, logger, adminService, authService);
        await builder.Create();

        // var su = SuperAdminBuilder.CreateSuperUser();
        // context.Add(su);
        // context.SaveChanges();
        Console.WriteLine("Database seeded");
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }
}