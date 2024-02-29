using Identity.Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace Identity.Services.Configuration;

internal class AuthOptions(IConfiguration configuration) : IAuthOptions
{
    public int RefreshTokenExpiration => int.Parse(configuration["Options:RefreshTokenExpiration"]);
    public int AccessTokenExpiration => int.Parse(configuration["Options:AccessTokenExpiration"]);
    public string PasswordRegex => configuration["Options:PasswordRegex"];
    public string EmailRegex => configuration["Options:EmailRegex"];
    public string AppNameRegex => configuration["Options:AppNameRegex"];
}