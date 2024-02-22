using Identity.Core;
using Identity.Persistence;
using Identity.Persistence.Database;
using Identity.Server.Tools;
using Identity.Services;
using Tools.TransactionsManager;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRouting(o => o.LowercaseUrls = true);
builder.Services.AddControllers();
builder.Services.AddCors(setup =>
{
    setup.AddPolicy("*", policyBuilder =>
    {
        policyBuilder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddPersistence(IdentityPersistanceProvider.BuildConnectionString(
    builder.Configuration["DbParams:Host"],
    builder.Configuration["DbParams:Port"],
    builder.Configuration["DbParams:Database"],
    builder.Configuration["DbParams:User"],
    builder.Configuration["DbParams:Password"]
));
builder.Services.AddTransactionManager<IdentityDb>();

builder.Services.AddIdentityDomain();
builder.Services.AddIdentityServices();

builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwagger();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("*");
app.UseRouting();
app.MapControllers();

app.UseSwaggerInterface();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.Run();