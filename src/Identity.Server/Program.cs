using System.Reflection;
using Identity.Core;
using Identity.Persistence;
using Identity.Persistence.Database;
using Identity.Server.Tools;
using Identity.Services;
using Tools.TransactionsManager;

namespace Identity.Server;

internal static class Program
{
    public static void Main(string[] args)
    {
        var appName = Assembly.GetExecutingAssembly().GetName().Name;

        var builder = WebApplication.CreateBuilder(args);

        Console.WriteLine($"Executing {appName}...");
        Console.WriteLine();

        LoadingServices(builder);
        var app = builder.Build();
        ConfigureMiddleware(app);

        Console.WriteLine("Starting...");
        app.Run();
    }

    private static void LoadingServices(IHostApplicationBuilder appBuilder)
    {
        Console.WriteLine("Loading Route and Controllers...");
        appBuilder.Services.AddRouting(o => o.LowercaseUrls = true);
        appBuilder.Services.AddControllers();
        appBuilder.Services.AddCors(setup =>
        {
            setup.AddPolicy("*", policyBuilder =>
            {
                policyBuilder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        appBuilder.Services.AddHttpContextAccessor();

        Console.WriteLine("Loading Persistence...");
        appBuilder.Services.AddPersistence(IdentityPersistenceProvider.BuildConnectionString(
            appBuilder.Configuration["DbParams:Host"],
            appBuilder.Configuration["DbParams:Port"],
            appBuilder.Configuration["DbParams:Database"],
            appBuilder.Configuration["DbParams:User"],
            appBuilder.Configuration["DbParams:Password"]
        ));
        appBuilder.Services.AddTransactionManager<IdentityDb>();

        Console.WriteLine("Loading Services...");
        appBuilder.Services.AddIdentityDomain();
        appBuilder.Services.AddJwtAuthentication(appBuilder.Configuration["Jwt:Key"]);
        appBuilder.Services.AddIdentityServices();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        appBuilder.Services.AddSwagger();
    }

    private static void ConfigureMiddleware(WebApplication webApplication)
    {
        // Configure the HTTP request pipeline.
        Console.WriteLine("Configuring HTTP request pipeline...");
        webApplication.UseCors("*");
        webApplication.UseRouting();
        webApplication.MapControllers();

        webApplication.UseSwaggerInterface();

        webApplication.UseAuthentication();
        webApplication.UseAuthorization();

        webApplication.UseHttpsRedirection();
    }
}