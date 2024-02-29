using Identity.Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace Identity.Services.Configuration;

internal class JwtOptions(IConfiguration configuration) : IJwtOptions
{
    public string Issuer => configuration["Jwt:Issuer"];

    public string Audience => configuration["Jwt:Audience"];
    public string Key => configuration["Jwt:Key"];
}