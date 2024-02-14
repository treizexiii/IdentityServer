using Identity.Core;
using Identity.Core.Entities;
using Identity.Persistence;
using Identity.Persistence.Database;
using Identity.Server.Tools;
using Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
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

var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = false,
    ValidateAudience = false,
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey("test"u8.ToArray())
};
// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = IdentityConstants.BearerScheme;
//     options.DefaultChallengeScheme = IdentityConstants.BearerScheme;
//     options.DefaultScheme = IdentityConstants.BearerScheme;
// }).AddJwtBearer(o =>
// {
//     o.TokenValidationParameters = tokenValidationParameters;
// });
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = tokenValidationParameters;

});
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

builder.Services.AddPersistence(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddTransactionManager<IdentityDb>();

builder.Services.AddIdentityDomain();
builder.Services.AddIdentityServices();
// builder.Services.AddIdentityCore<User>();
//     .AddApiEndpoints();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwagger();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("*");
app.UseRouting();
app.MapControllers();
//
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// app.MapIdentityApi<User>();
app.UseSwaggerInterface();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.Run();