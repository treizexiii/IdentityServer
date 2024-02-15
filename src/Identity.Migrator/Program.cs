// See https://aka.ms/new-console-template for more information

using Identity.Migrator;
using Identity.Persistence;
using Identity.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, Migrator!");

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
Console.WriteLine("Environment: " + env);

DotEnv.Load(".env");
var host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(builder =>
    {
        builder.AddJsonFile("appsettings.json", optional: true);
        builder.AddJsonFile($"appsettings.{env}.json", optional: true);
        builder.AddEnvironmentVariables();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddPersistence(IdentityPersistanceProvider.BuildConnectionString(
            hostContext.Configuration["DB_HOST"],
            hostContext.Configuration["DB_PORT"],
            hostContext.Configuration["DB_NAME"],
            hostContext.Configuration["DB_USER"],
            hostContext.Configuration["DB_PASSWORD"]
        ));
    })
    .Build();

var command = args[0];

switch (command)
{
    case "migrate":
        Migrate(host.Services.GetRequiredService<IdentityDb>());
        break;
    case "seed":
        Seed(host.Services.GetRequiredService<IdentityDb>());
        break;
    case "create":
        Create(host.Services.GetRequiredService<IdentityDb>());
        break;
    case "drop":
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

void Seed(DbContext context)
{
    try
    {
        // var roles = RolesList.GetRoles();
        // context.AddRange(roles);
        var su = SuperAdminBuilder.Create();
        context.Add(su);
        context.SaveChanges();
        Console.WriteLine("Database seeded");
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }
}