// See https://aka.ms/new-console-template for more information

using Identity.Core;
using Identity.Persistence;
using Identity.Persistence.Database;
using Identity.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
        var host = BuildHost(args);

        Console.WriteLine("Starting migrator...");
        var app = new Application(host);
        await app.RunAsync(args);
    }

    private static IHost BuildHost(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
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
                services.AddPersistence(connectionString);
                services.AddTransactionManager<IdentityDb>();
                services.AddIdentityDomain();
                services.AddIdentityServices();
            })
            .Build();

        return host;
    }
}